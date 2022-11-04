using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    Rigidbody rb;
    PlayerCont pc;
    public LayerMask whatIsWall;

    [Header("Climning")]
    public float climbSpeed;
    public float maxClimbSTime;
    private float climbTimer;

    [Header("ClimingJump")]
    public float climbingJumpUpForce;
    public float climbingJumpBackForce;
    public KeyCode jumpKey=KeyCode.Space;
    public int climbJump;
    int climbJumpLeft;

    [Header("Detection")]
    public float detectionLenght;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    float wallLookAngel;

    [Header("Exiting")]
    public bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    private RaycastHit frontWallHit;
    bool wallFront;

    private Transform lastWall;
    Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    public bool climbing;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pc = GetComponent<PlayerCont>();
    }

    // Update is called once per frame
    void Update()
    {
        WallCheck();
        StateMachine();
        if (climbing &&!exitingWall)
        {
            ClimbingMovemnt();
        }
    }
    private void StateMachine()
    {
        if (wallFront&&Input.GetKey(KeyCode.W)&&wallLookAngel<maxWallLookAngle && !exitingWall)
        {
            
            if (!climbing && climbTimer>0)
            {
                StartClimbing();
            }
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0) StopClimng();
        }
        else if (exitingWall)
        {
            if (climbing) StopClimng();
            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingWall = false;
        }

        else
        {
            if (climbing) StopClimng();
        }
        if (wallFront&&Input.GetKeyDown(jumpKey)&&climbJumpLeft>0)
        {
            ClimbJump();
        }
    }
    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLenght, whatIsWall);
        wallLookAngel = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal))>minWallNormalAngleChange;
        if ((wallFront&&newWall) ||pc.grounded)
        {
            climbTimer = maxClimbSTime;
            climbJumpLeft = climbJump;
        }
    }
    public void StartClimbing()
    {
        climbing = true;
        pc.climbing =true;

        lastWall = frontWallHit.transform;
        lastWallNormal=frontWallHit.normal;
    }
    void ClimbingMovemnt()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }
    void StopClimng()
    {
        climbing = false;
        pc.climbing = false;
    }
    void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbingJumpUpForce + frontWallHit.normal * climbingJumpBackForce;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpLeft--;
    }
}
