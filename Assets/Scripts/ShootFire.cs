using UnityEngine;

public class ShootFire : MonoBehaviour
{
    public Camera Cam;
    public HealthComponent healthComponent;

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && Cam != null)
        {
            FireCast();
        }
    }

    void FireCast()
    {
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out RaycastHit hit, 100))
        {
            Debug.Log(hit.transform.name);
            Debug.DrawRay(Cam.transform.position, Cam.transform.forward * 100, Color.red, 2.0f);

            if (hit.transform.CompareTag("Enemy"))
            {
                healthComponent.ApplyDamage(10f);
            }


        }
    }


}
