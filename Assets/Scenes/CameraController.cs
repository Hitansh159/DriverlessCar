using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Transform target;
    public Vector3 offset;
    public Transform lookAt;
    public float smoothSpeed = 0.125f; 

    // Start is called before the first frame update
    void Start()
    {
        transform.position = target.position + offset;    
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);      

        transform.LookAt(lookAt.position);        
    }
}
