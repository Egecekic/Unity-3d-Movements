using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRuning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimebSpeed;
    public float maxWallRunTÝme;
    float wallRunT;

    [Header("Input")]
    public KeyCode uperWardsKey = KeyCode.LeftShift;
    public KeyCode downWardsKey = KeyCode.LeftControl;
    private bool uperWardsRunning;
    private bool downerWardsRunning;
    public float HorizontalInput;
    public float VerticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orienctation;
    Rigidbody rb;
    PlayerCont playerCont;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCont = GetComponent<PlayerCont>();
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();
        StateMachine();
        
    }
    private void FixedUpdate()
    {
        if (playerCont.wallRunning)
        {
            wallRunningMovement();
        }
    }
    public void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orienctation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orienctation.right, out leftWallhit, wallCheckDistance, whatIsWall);

    }
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHight, whatIsGround);
    }
    public void StateMachine()
    {
        //Get Input
        HorizontalInput = Input.GetAxisRaw("Horizontal");
        VerticalInput = Input.GetAxisRaw("Vertical");

        uperWardsRunning = Input.GetKey(uperWardsKey);
        downerWardsRunning = Input.GetKey(downWardsKey);

        //State 1 - WallRinning
        if ((wallLeft || wallRight) && VerticalInput > 0 && AboveGround())
        {
            startWallRunning();
        }
        else
        {
            stopWallRunning();
        }
    }
    public void startWallRunning()
    {
        playerCont.wallRunning = true;

    }
    public void wallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal ;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        if ((orienctation.forward-wallForward).magnitude>(orienctation.forward- -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        if (uperWardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, wallClimebSpeed);
        }
        if (downerWardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, -wallClimebSpeed);
        }

        //push to wall
        if ((wallLeft&&HorizontalInput>0)&& !(wallRight&&HorizontalInput<0))
        {
            rb.AddForce(-wallForward * 100, ForceMode.Force);
        }
        
    }
    public void stopWallRunning()
    {
        rb.useGravity = true;
        playerCont.wallRunning = false;
    }
}
