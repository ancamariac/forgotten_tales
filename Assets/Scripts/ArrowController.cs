using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : NetworkBehaviour
{
    public Vector3 target;

    const float speed = 2f;
    const float aliveTime = 5f;

    public CharacterController shooter;

    private void Start()
    {
        // Set random up direction so the back fins are randomized ( not in the same orientation )
        Vector3 randomUpDirection = new Vector3(Random.Range(0, 360),
            Random.Range(0, 360),
            Random.Range(0, 360));

        transform.LookAt(target, randomUpDirection);

        if (isServer)
        {
            Invoke(nameof(AutoDestroy), aliveTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed);
        }
    }

    [Server]
    void AutoDestroy()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider co)
    {
        AIController mobAI = co.gameObject.GetComponent<AIController>();

        if (mobAI == null)
        {
            return;
        }

        if (IsInvoking(nameof(AutoDestroy)))
        {
            CancelInvoke(nameof(AutoDestroy));
        }

        float hpLeft = mobAI.TakeDamage(10f);

        if (hpLeft <= 0f)
        {
            shooter.IncreaseExp(10f);
        }

        AutoDestroy();
    }
}
