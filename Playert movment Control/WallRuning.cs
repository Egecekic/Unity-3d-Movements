using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WallRuning : NetworkBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    // Duvar ko�usu s�ras�nda oyuncuya uygulanan itme kuvveti
    public float wallRunForce;
    // Duvara ko�arken yukar� do�ru yap�lan z�plamada uygulanan yukar� y�nl� kuvvet
    public float wallJumpUpForce;
    public float wallJumpSideForce;// Duvara ko�arken yana do�ru yap�lan z�plamada uygulanan yana y�nl� kuvvet
    public float wallClimebSpeed;// Duvara ko�arken yukar� t�rmanma h�z�n� belirten de�er
    public float maxWallRunTime;// Maksimum duvar ko�usu s�resi (saniye cinsinden)
    float wallRunTimer;// Duvar ko�usu s�ras�nda zamanlay�c� i�in kullan�lan de�i�ken

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode uperWardsKey = KeyCode.LeftShift;
    public KeyCode downWardsKey = KeyCode.LeftControl;
    private bool uperWardsRunning;
    private bool downerWardsRunning;
    float HorizontalInput;
    float VerticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orienctation;
    public PlayerCam cam;
    
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
        if (!IsOwner) return;
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
        if ((wallLeft || wallRight) && VerticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!playerCont.wallRunning)
            {
                startWallRunning();
            }
            if (wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }
            if (wallRunTimer <= 0 && playerCont.wallRunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }
            if (Input.GetKeyDown(jumpKey))
            {
                wallJump();
            }
        }
        else if (exitingWall)
        {
            if (playerCont.wallRunning)
            {
                stopWallRunning();
            }
            if (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            
            if (exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }
        else
        {
            stopWallRunning();
        }
    }
    public void startWallRunning()
    {
        playerCont.wallRunning = true;
        wallRunTimer = maxWallRunTime;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        //apply camera effects
        cam.DoFov(90);
        if (wallLeft)
        {
            cam.DoTile(-5f);
        }
        if (wallRight) cam.DoTile(5f);
    }
    public void wallRunningMovement()
    {
        rb.useGravity = useGravity;


        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        if ((orienctation.forward - wallForward).magnitude > (orienctation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        if (uperWardsRunning)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimebSpeed);
        }
        if (downerWardsRunning)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimebSpeed);
        }

        //push to wall
        if ((wallLeft && HorizontalInput > 0) && !(wallRight && HorizontalInput < 0))
        {
            rb.AddForce(-wallForward * 100, ForceMode.Force);
        }
        //weaken Gravity
        if (useGravity)
        {
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);

        }

    }
    public void stopWallRunning()
    {
        rb.useGravity = true;
        playerCont.wallRunning = false;
        cam.DoFov(80);
        cam.DoTile(0f);
    }
    public void wallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpSideForce + wallNormal * wallJumpSideForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}