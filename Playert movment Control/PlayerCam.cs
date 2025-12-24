using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Netcode;

public class PlayerCam : NetworkBehaviour
{
    public float sensX;
    public float sensY;
    public float upYLim, downYLim;

    public Transform orientation;
    public Transform cameHolder;

     float xRotation;
     float yRotation;
    // Normalized mouse input deðerleri
    [SerializeField] private float normalizedMouseX;
    [SerializeField] private float normalizedMouseY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (!IsLocalPlayer)
        {
            GetComponentInChildren<Camera>().enabled = false;
        }
    }

    void Update()
    {
        if (!IsOwner) return;
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;
        //Debug.Log(transform.rotation.);
        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, downYLim, upYLim);
        /*
        cameHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        */
        Quaternion targertRotation = Quaternion.Euler(xRotation, yRotation, 0);
        cameHolder.rotation = Quaternion.Slerp(cameHolder.rotation, targertRotation, Time.deltaTime * 10f);
        // Normalize etme iþlemi (-1 ile 1 arasýna getiriyoruz)
        normalizedMouseX = Mathf.Clamp(mouseX, -1f, 1f);
        normalizedMouseY = Mathf.Clamp(mouseY, -1f, 1f);
    }
    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }
    public void DoTile(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
    public float GetYRotation()
    {
        return yRotation;
    }
    public float GetXNormalized()
    {
        return normalizedMouseX;
    }

}