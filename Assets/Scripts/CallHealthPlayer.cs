using UnityEngine;
using System.Collections;

public class CallHealthPlayer : MonoBehaviour
{
    HealthComponent HealthComponent { get; set; }
    public GameObject Screen;

    private void Awake()
    {

        HealthComponent = GetComponent<HealthComponent>();
        HealthComponent.OnHealthEmpty += OnHealthEmpty;
    }

    private void OnHealthEmpty()
    {

        Screen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


    }
}
