using UnityEngine;

public class EnemyStatesAttack : MonoBehaviour
{
    public HealthComponent healthComponent;
    readonly System.Random m_random = new();
    int randomAttack;
    private Animation anim;
    public float CurrentHealth { get; set; }
    public bool IsAlive => CurrentHealth > 0.0f;

    void Start()
    {
        anim = gameObject.GetComponent<Animation>();

    }


    void OnTriggerStay(Collider other)
    {

        //if (other.CompareTag("Player") && IsAlive)
        if (other.CompareTag("Player"))
        {
            anim.CrossFade("attack_short_001", 0.0f);
            anim.CrossFadeQueued("idle_combat");
            Attack();
        }
    }

    void Attack()
    {
        randomAttack = m_random.Next(1, 150);

        if (randomAttack == 1)
        {
            healthComponent.ApplyDamage(1f);
        }


    }
}
