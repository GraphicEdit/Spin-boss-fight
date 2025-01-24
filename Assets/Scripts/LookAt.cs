using UnityEngine;

public class LookAt : MonoBehaviour
{
    private Transform _camera;


    void Start()
    {

        _camera = Camera.main.transform;

    }


    void Update()
    {

        transform.LookAt(_camera);
    }
}
