using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraMove : NetworkBehaviour
{
    public Transform targetPosition;  // Aim.cs bunu deðiþtirecek
    public float smoothSpeed = 5f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (targetPosition != null)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition.position, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetPosition.rotation, Time.deltaTime * smoothSpeed);
        }
    }
}
