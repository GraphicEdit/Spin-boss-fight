using UnityEngine;
using System.Collections;

public class DeadPlayer : MonoBehaviour
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
