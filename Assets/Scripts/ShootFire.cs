using UnityEngine;

public class ShootFire : MonoBehaviour
{
    public Camera Cam;
    public GameObject VFX;
    public HealthComponent healthComponent;
    public AudioSource audioSource;

    public GameObject projectile;
    public float initialVelocity;

    readonly System.Random m_random = new();
    int randomAttack;

    void Update()
    {

        if (Input.GetMouseButtonDown(1) && Cam != null)
        {
            FireCast();
            VFX.SetActive(true);
            audioSource.Play();
            Invoke(nameof(VFXstop), 0.2f);
        }
    }

    void FireCast()
    {
        if (Physics.Raycast(Cam.transform.position, Cam.transform.forward, out RaycastHit hit, 100))
        {
            //Debug.Log(hit.transform.name);
            //Debug.DrawRay(Cam.transform.position, Cam.transform.forward * 100, Color.red, 2.0f);
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Projectile"))
            {
                GameObject.Destroy(obj);
            }
            var projectileInstance = GameObject.Instantiate(projectile, this.transform.position, Quaternion.identity);
            projectileInstance.GetComponent<Rigidbody>().linearVelocity = initialVelocity * this.transform.forward;

            /* if (hit.transform.CompareTag("Enemy"))
             {
                 healthComponent.ApplyDamage(10f);
             }
             */
            if (hit.transform.CompareTag("Enemy"))
            {
                randomAttack = m_random.Next(1, 10);

                if (randomAttack == 1)
                {
                    healthComponent.ApplyDamage(50f);
                }
            }

        }

    }


    void VFXstop()
    {
        VFX.SetActive(false);
    }

}
