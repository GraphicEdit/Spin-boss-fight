using UnityEngine;



[AddComponentMenu("Health System/Health")]
public class Health : HealthComponent
{

    [SerializeField]
    private float maxHealth = 100.0f;

    [SerializeField, Range(0, 1)]
    private float initialRatio = 1.0f;

    public override float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public override float CurrentHealth { get; set; } = 0.0f;
    public override bool IsAlive => CurrentHealth > 0.0f;

    private void Awake()
    {
        SetHealth(maxHealth * initialRatio);

    }


    public override void SetHealth(float health)
    {
        float previousHealth = CurrentHealth;

        CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);

        float difference = health - previousHealth;

        if (difference > 0.0f)
        {
            OnHealthChanged?.Invoke(difference);
        }
    }


    public override void AddHealth(float amount)
    {
        if (IsAlive == false)
        {
            return;
        }

        float previousHealth = CurrentHealth;

        CurrentHealth += amount;

        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        float changeAmount = CurrentHealth - previousHealth;

        if (changeAmount > 0.0f)
        {
            OnHealthChanged?.Invoke(changeAmount);
        }
    }


    public override void ApplyDamage(float damage)
    {
        if (IsAlive == false)
        {
            return;
        }

        float previousHealth = CurrentHealth;

        CurrentHealth = Mathf.Clamp(CurrentHealth - damage, 0, MaxHealth);

        float changeAmount = CurrentHealth - previousHealth;

        if (Mathf.Abs(changeAmount) > 0.0f)
        {
            OnHealthChanged?.Invoke(changeAmount);

            if (CurrentHealth <= 0.0f)
            {
                OnHealthEmpty?.Invoke();
            }
        }
    }


}
