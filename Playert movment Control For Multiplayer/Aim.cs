using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Aim : NetworkBehaviour
{
    public Transform idleaPos, aimPos;
    public CameraMove cameraMove;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetMouseButtonDown(1)) // Corrected method and argument
        {
            cameraMove.targetPosition = aimPos;
        }
        if (Input.GetMouseButtonUp(1)) // Corrected method and argument
        {
            cameraMove.targetPosition = idleaPos;
        }
    }
}
