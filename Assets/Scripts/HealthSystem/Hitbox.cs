using UnityEngine;



[RequireComponent(typeof(Collider))]
[AddComponentMenu("Health System/Hitbox")]
public class Hitbox : HitboxComponent
{
    [field: SerializeField]
    public override HealthComponent Health { get; protected set; }

    [Range(0f, 1f)]
    public float damageMultiplier = 1.0f;

    void Reset()
    {
        if (Health == null)
        {
            Health = GetComponentInParent<HealthComponent>();
        }
    }

    public override void ApplyDamage(float damageAmount)
    {
        if (isActiveAndEnabled && Health != null)
        {
            Health.ApplyDamage(damageAmount * damageMultiplier);
        }
    }
}
