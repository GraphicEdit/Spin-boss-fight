using UnityEngine;


public abstract class HitboxComponent : MonoBehaviour
{
    public abstract HealthComponent Health { get; protected set; }

    public abstract void ApplyDamage(float damageAmount);
}
