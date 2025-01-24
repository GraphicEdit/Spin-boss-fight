using UnityEngine;


public class CallHealth : MonoBehaviour
{
    HealthComponent HealthComponent { get; set; }

    private void Awake()
    {
        HealthComponent = GetComponent<HealthComponent>();
        HealthComponent.OnHealthEmpty += OnHealthEmpty;
    }

    private void OnHealthEmpty()
    {
        Destroy(gameObject, 1.0f);


    }
}
