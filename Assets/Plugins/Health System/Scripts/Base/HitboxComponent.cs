using UnityEngine;

namespace HealthSystem
{

    public abstract class HitboxComponent : MonoBehaviour
    {
        public abstract HealthComponent Health { get; protected set; }

        public abstract void ApplyDamage(float damageAmount);
    }
}