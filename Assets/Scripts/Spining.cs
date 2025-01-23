using UnityEngine;

public class Spining : MonoBehaviour
{

    [SerializeField] Transform PlatformA;
    [SerializeField] Transform PlatformB;
    [SerializeField] Transform PlatformC;


    void Update()
    {
        // Rotate PlatformA continuously
        PlatformA.Rotate(Vector3.up * Time.deltaTime * -25);
        PlatformB.Rotate(Vector3.up * Time.deltaTime * 30);
        PlatformC.Rotate(Vector3.up * Time.deltaTime * -20);

    }
}
