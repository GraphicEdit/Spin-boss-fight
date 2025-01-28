using UnityEngine;
using System.Collections;

public class EnemyStatesAttack : MonoBehaviour
{
    public HealthComponent healthComponent;
    readonly System.Random m_random = new();
    int randomAttack;
    private Animation anim;
    public GameObject DamageVFX;

    void Start()
    {
        anim = gameObject.GetComponent<Animation>();
    }


    void OnTriggerStay(Collider other)
    {

        //if (other.CompareTag("Player") && IsAlive)
        if (other.CompareTag("Player"))
        {
            //anim.CrossFade("attack_short_001", 0.0f);
            anim.Play("attack_short_001");
            //anim.CrossFadeQueued("idle_combat");
            Attack();
        }
    }

    void Attack()
    {
        randomAttack = m_random.Next(1, 150);

        if (randomAttack == 1)
        {
            healthComponent.ApplyDamage(1f);

            DamageVFX.SetActive(true);
            StartCoroutine(waiter());
        }


    }

    private IEnumerator waiter()
    {
        yield return new WaitForSeconds(.1f);
        DamageVFX.SetActive(false);


    }

}
