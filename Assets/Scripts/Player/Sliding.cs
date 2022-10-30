using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("Referencs")]
    public Transform orientation;
    public Transform playerOb;
    private Rigidbody rb;
    private PlayerCont playerCont;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForece;
    public float slideTimer;

    

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCont = GetComponent<PlayerCont>();

        startYScale=playerOb.transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey)&&(horizontalInput !=0 || verticalInput!=0))
        {
            StartSlide();
        }
        if (Input.GetKeyUp(slideKey)&&playerCont.sliding)
        {
            StopSliding();
        }
        
    }
    private void FixedUpdate()
    {
        if (playerCont.sliding)
        {
            SlidingMovemnt();
        }
    }

    public void StartSlide()
    {
        playerCont.sliding = true;
        playerOb.localScale=new Vector3(playerOb.localScale.x,slideYScale,playerOb.localScale.z);
        rb.AddForce(Vector3.down * 10f, ForceMode.Force);

        slideTimer = maxSlideTime;
    }
    public void SlidingMovemnt()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;


        if (!playerCont.OnSlope()||rb.velocity.y> -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForece, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        else
        {
            rb.AddForce(playerCont.GetSlopeMoveDirection(inputDirection) * slideForece, ForceMode.Force);
        }

        if (slideTimer<=0)
        {
            StopSliding();
        }
    }
    public void StopSliding()
    {
        playerCont.sliding = false;
        playerOb.localScale = new Vector3(playerOb.localScale.x, startYScale, playerOb.localScale.z);

    }
}
