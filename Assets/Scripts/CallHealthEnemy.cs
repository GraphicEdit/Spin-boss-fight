using UnityEngine;
using System.Collections;


public class CallHealthEnemy : MonoBehaviour
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

        Destroy(GetComponent<CapsuleCollider>());

        GetComponent<Animation>().Stop();
        GetComponent<Animation>().CrossFade("dead", 0.2f);

        StartCoroutine(waiter());
        //Debug.Log(".................................");

    }
    private IEnumerator waiter()
    {
        //Wait for 4 seconds
        yield return new WaitForSeconds(5);
        Screen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //Time.timeScale = 0;

    }

}
