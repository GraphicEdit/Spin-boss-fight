using UnityEngine;

public class LookAt : MonoBehaviour
{
    private Transform _target;


    void Start()
    {

        _target = Camera.main.transform;

    }


    void Update()
    {
        transform.LookAt(new Vector3(_target.position.x, transform.position.y, _target.position.z));
        //transform.LookAt(_target);
    }
}
