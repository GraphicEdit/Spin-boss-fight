﻿using UnityEngine;
using UnityEngine.Events;



public abstract class HealthComponent : MonoBehaviour
{
    public UnityAction<float> OnHealthChanged { get; set; }
    public UnityAction OnHealthEmpty { get; set; }
    public abstract float MaxHealth { get; set; }
    public abstract float CurrentHealth { get; set; }
    public abstract bool IsAlive { get; }

    public abstract void AddHealth(float amount);
    public abstract void ApplyDamage(float damage);
    public abstract void SetHealth(float health);
}
