using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : NetworkBehaviour
{
    private const float lifetime = 1f;

    public CharacterController shooter;

    void Start()
    {
        if( isServer)
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
        
        mobAI.RpcTakeDamage(10f);
        Debug.Log("-----------------HAM-------------------");
        if (mobAI.GetHealth() <= 10f)
        {
            Debug.Log("-----------------MIAU-------------------");
            shooter.IncreaseExp(10f);
        }
    }
}