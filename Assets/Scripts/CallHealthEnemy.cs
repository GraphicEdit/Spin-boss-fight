using UnityEngine;


public class CallHealthEnemy : MonoBehaviour
{
    HealthComponent HealthComponent { get; set; }


    private void Awake()
    {

        HealthComponent = GetComponent<HealthComponent>();
        HealthComponent.OnHealthEmpty += OnHealthEmpty;

    }

    private void OnHealthEmpty()
    {
        //Destroy(gameObject, 1.0f);
        Destroy(GetComponent<CapsuleCollider>());
        GetComponent<Animation>().Stop();
        GetComponent<Animation>().CrossFade("dead", 0.2f);
        //Debug.Log(".................................");



    }
}
