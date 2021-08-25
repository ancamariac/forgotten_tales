using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTrail : NetworkBehaviour
{
    private const float lifetime = 1f;

    public CharacterController shooter;

    void Start()
    {
        if (isServer)
        {
            Invoke(nameof(EndOfLife), lifetime);
        }
    }

    private void EndOfLife()
    {
        NetworkServer.Destroy(gameObject);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider co)
    {
        AIController mobAI = co.gameObject.GetComponent<AIController>();

        if (mobAI == null)
        {
            return;
        }

        float hpLeft = mobAI.TakeDamage(5f);

        if (hpLeft <= 0f)
        {
            shooter.IncreaseExp(10f);
        }
    }
}
