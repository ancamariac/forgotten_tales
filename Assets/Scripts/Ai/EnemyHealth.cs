using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class EnemyHealth : NetworkBehaviour
{
    [SerializeField]
    public float maxHealth = 100;

    [SyncVar(hook = nameof(SetCurrentHealth))]
    [SerializeField]
    private float currentHealth;

    [SerializeField]
    public HealthBar healthBar;

    public float CurrentHealth { get { return currentHealth; } set { currentHealth = value; } }

    void Start()
    {
        CurrentHealth = maxHealth;
        healthBar.FillAmount = maxHealth;
    }

    void SetCurrentHealth(float oldHealth, float newHealth)
    {
        if (isClient)
        {
            DamageBarUpdate();
        }
    }

    public void DamageBarUpdate()
    {
        float currentHealthPercent = (float)currentHealth / (float)maxHealth;
        healthBar.FillAmount = currentHealthPercent;
    }


}
