using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : NetworkBehaviour
{
    public HumanoidRigidRig rigParts;

    private Rigidbody rb;

    public CharacterInputFeed cif;

    private ProceduralAnimationController<HumanoidRigidRig> animationController;
    private MovementController movementController;

    private ProceduralAnimation<HumanoidRigidRig> walkAnim;
    private ProceduralAnimation<HumanoidRigidRig> idleAnim;
    private ProceduralAnimation<HumanoidRigidRig> attackAnim;

    private CharacterOutfitSync characterOutfit;

    private float cooldownTime = 0.6f;
    private float nextAttack = 0;

    const float rehealRate = 0.7f;
    const float rehealPeriod = 0.7f;

    const float manaRestoreRate = 0.7f;
    const float manaRestorePeriod = 0.7f;

    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject swordTrailPrefab;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private Slider expSlider;

    private GameObject badlyDamaged;

    private PlayerHealthBar hpbar;

    void Start()
    {
        CameraController camController;

        camController = Camera.main.GetComponent<CameraController>();

        rb = GetComponent<Rigidbody>();

        characterOutfit = GetComponent<CharacterOutfitSync>();

        hpbar = transform.Find("HealthBar").gameObject.GetComponent<PlayerHealthBar>();

        if (isLocalPlayer)
        {
            cif = new LocalKeyboardCIF(camController);
            camController.SetCameraTarget(transform);
            HumanoidRigInitialPose.SetupInstance(rigParts);

            characterOutfit.LocalInit();

            hpbar.gameObject.SetActive(false);
        }
        else
        {
            cif = GetComponent<CIFSync>();// new NetworkedCIF();
        }

        animationController = new ProceduralAnimationController<HumanoidRigidRig>(cif, rigParts);

        walkAnim = new HumanWalk(rigParts, cif);
        attackAnim = new HumanAttack(rigParts, cif);
        idleAnim = new HumanIdle(rigParts, cif);

        animationController.SwitchTo(walkAnim);
        //animationController.SwitchTo(attackAnim);

        movementController = new MovementController(rb, cif);

        healthSlider = GameObject.Find("Canvas").transform
            .Find("Health Bar").gameObject.GetComponent<Slider>();

        badlyDamaged = GameObject.Find("Canvas").transform
            .Find("BadlyDamaged").gameObject;

        manaSlider = GameObject.Find("Canvas").transform
            .Find("Mana Bar").gameObject.GetComponent<Slider>();

        expSlider = GameObject.Find("Canvas").transform
            .Find("Exp Bar").gameObject.GetComponent<Slider>();

        if (isServer)
        {
            InvokeRepeating(nameof(HealingTick), rehealPeriod, rehealRate);
            InvokeRepeating(nameof(ManaRestoreTick), manaRestorePeriod, manaRestoreRate);
        }
    }

    private void Update()
    {


        if (health < 10f)
        {
            badlyDamaged.SetActive(true);
        }
        else
        {
            badlyDamaged.SetActive(false);
        }

        if (Time.time > nextAttack)
        {
            if (cif.AttemptsAttack())
            {
                animationController.SwitchTo(attackAnim);

                if (isLocalPlayer)
                {
                    Debug.Log("Cmd attack - clientside");

                    if (characterOutfit.GetClassIndex() == (int)CharacterClass.Knight)
                    {
                        CmdKnightAttack();
                    }

                    if (characterOutfit.GetClassIndex() == (int)CharacterClass.Mage)
                    {
                        CmdMageAttack();
                    }

                    if (characterOutfit.GetClassIndex() == (int)CharacterClass.Archer)
                    {
                        Vector3 target;
                        if (BowRaycast(out target))
                        {
                            CmdArcheryAttack(target);
                        }
                    }
                }

                nextAttack = Time.time + cooldownTime;
            }
        }

        if (animationController.GetCurrentAnim() == attackAnim)
        {
            if (animationController.Finished())
            {
                animationController.SwitchTo(walkAnim);
            }
        }

        if (animationController.GetCurrentAnim() == walkAnim)
        {
            if (!cif.IsWalking() && !cif.IsWalkingBackwards())
            {
                if (animationController.Finished())
                    animationController.SwitchTo(idleAnim);
            }
        }

        if (animationController.GetCurrentAnim() == idleAnim)
        {
            if (cif.JustStartedWalking())
            {
                animationController.SwitchTo(walkAnim);
            }
        }


        animationController.Step(Time.deltaTime);

        if (isLocalPlayer)
        {
            movementController.Step(Time.deltaTime);
        }
    }

    [Command]
    private void CmdKnightAttack()
    {
        Debug.Log("Cmd attack - serverside");
        if (characterOutfit.GetClassIndex() == (int)CharacterClass.Knight)
        {
            Vector3 spawnPosition = transform.position + transform.rotation * Vector3.forward * 1f;
            GameObject swordTrail = Instantiate(swordTrailPrefab, spawnPosition, transform.rotation);
            NetworkServer.Spawn(swordTrail);

            swordTrail.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, 1), ForceMode.Impulse);
            swordTrail.GetComponent<SwordTrail>().shooter = this;
        }
    }

    [Command]
    private void CmdMageAttack()
    {
        Debug.Log("Cmd attack - serverside");
        if (characterOutfit.GetClassIndex() == (int)CharacterClass.Mage && mana > 4f)
        {
            Vector3 spawnPosition = transform.position + transform.rotation * Vector3.forward * 2f;
            GameObject fireball = Instantiate(fireballPrefab, spawnPosition, transform.rotation);
            NetworkServer.Spawn(fireball);

            fireball.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 5, 10), ForceMode.Impulse);
            fireball.GetComponent<Fireball>().shooter = this;

            ManaUsage(4f);
        }
    }

    [Command]
    private void CmdArcheryAttack(Vector3 raycastedTarget)
    {
        if (characterOutfit.GetClassIndex() == (int)CharacterClass.Archer && mana > 4f)
        {
            Vector3 spawnPosition = transform.position + transform.rotation * Vector3.forward * 1f;
            GameObject arrow = Instantiate(arrowPrefab, spawnPosition, transform.rotation);
            NetworkServer.Spawn(arrow);

            arrow.GetComponent<ArrowController>().target = raycastedTarget;
            arrow.GetComponent<ArrowController>().shooter = this;

            ManaUsage(4f);
        }
    }

    // Raycasts a ray from the camera middle point in the scene and returns contact point
    private bool BowRaycast(out Vector3 point)
    {
        Ray rayOrigin = Camera.main.ScreenPointToRay(
            new Vector3(Camera.main.scaledPixelWidth / 2, Camera.main.scaledPixelHeight / 2, 0));

        var results = Physics.RaycastAll(rayOrigin, 100f);

        foreach (var i in results)
        {
            if (i.collider != null && i.collider.gameObject != gameObject)
            {
                //Vector3 direction = rayinfo.point - Camera.main.transform.position;

                point = i.point;
                return true;
            }
        }

        point = Vector3.zero;
        return false;
    }

    [SyncVar(hook = nameof(SetHealth))]
    private float health = 50f;

    [SyncVar(hook = nameof(SetMana))]
    private float mana = 40f;

    [SyncVar(hook = nameof(SetExp))]
    public float experience = 0f;

    void SetHealth(float oldHealth, float newHealth)
    {
        if (isLocalPlayer)
        {
            healthSlider.value = newHealth;
        }
        else
        {
            hpbar.FillAmount = health / healthSlider.maxValue;
        }
    }

    void SetMana(float oldMana, float newMana)
    {
        if (isLocalPlayer)
        {
            manaSlider.value = newMana;
        }
    }

    void SetExp(float oldExp, float newExp)
    {
        if (isLocalPlayer)
        {
            expSlider.value = newExp;
        }
    }

    [Server]
    public void DealDamage(float damage)
    {
        health -= damage;

        if (health < 0f)
        {
            health = 0f;
        }
    }

    [Server]
    public void ManaUsage(float power)
    {
        mana -= power;

        if (mana < 0f)
        {
            mana = 0f;
        }
    }

    [Server]
    public void IncreaseExp(float xp)
    {
        experience += xp;
    }

    [Server]
    private void HealingTick()
    {
        health += rehealRate;
        if (health > healthSlider.maxValue)
        {
            health = healthSlider.maxValue;
        }
    }

    [Server]
    private void ManaRestoreTick()
    {
        mana += manaRestoreRate;
        if (mana > manaSlider.maxValue)
        {
            mana = manaSlider.maxValue;
        }
    }
}