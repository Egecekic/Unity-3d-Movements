using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCont : MonoBehaviour
{
   [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float sliderSpeed;
    public float climbingSpeed;
    public float wallRunSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float groundDrag;

    [Header("Crouching")]
    public bool flag;
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public GameObject crouchingPoint;

    [Header("Slope Handling")]
    public float masSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool readyToJump;
    [Header("References")]
    public Climbing climbingScript;

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;


    [Header("Ground Checl")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;
    Vector3 moveDriction;

    public MovementState state;

    public enum MovementState { 
        walking,
        sprinting,
        crouching,
        climbing,
        wallRunning,
        sliding,
        air
    };

    public bool sliding;
    public bool climbing;
    public bool wallRunning;

    float horizontalInput;
    float verticalInput;

    Vector3 direction;
    Rigidbody Rigidbody;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.freezeRotation = true;
        climbingScript = GetComponent<Climbing>();

        readyToJump = true;

        startYScale = transform.localScale.y;
        
    }

    void Update()
    {

        CrouchPoint();
        grounded =Physics.Raycast(transform.position,Vector3.down,playerHeight*0.5f+0.2f,whatIsGround);

        MyInput();
        StateHandler();
        SpeedControl();
        if (grounded)
        {
            Rigidbody.drag = groundDrag;
        }
        else
        {
            Rigidbody.drag = 0;
        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down*1.2f);
    }

    public void StateHandler()
    {
        if (climbingScript.exitingWall)
        {
            return;
        }
        //Climb
        if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbingSpeed;
        }
        //wall Runn
        if (wallRunning)
        {
            state = MovementState.wallRunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        //slide
        else if (sliding)
        {
            state = MovementState.sliding;
            if (OnSlope()&& Rigidbody.velocity.y<0.1f)
            {
                desiredMoveSpeed = sliderSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
            
        }
        //Hýzlý Kosma
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        //Yürüme
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        //Crouch
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else
        {
            state = MovementState.air;
        }
        //Debug.Log(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed));
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed)>5f &&  moveSpeed !=0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }
        Debug.Log(moveSpeed);

        lastDesiredMoveSpeed = desiredMoveSpeed;
        
    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float diffrence = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time<diffrence)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / diffrence);
            time += Time.deltaTime;
            
            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
        Debug.Log(moveSpeed);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    public void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey)&&readyToJump&&grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            Rigidbody.AddForce(Vector3.down, ForceMode.Impulse);
        }
        if (Input.GetKeyUp(crouchKey)&&!flag)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            
        }

    }
    public void MovePlayer()
    {
        direction = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (OnSlope()&&!exitingSlope)
        {
            Rigidbody.AddForce(GetSlopeMoveDirection(direction)*moveSpeed*20f,ForceMode.Force);
            if (Rigidbody.velocity.y>0)
            {
                Rigidbody.AddForce(Vector3.down*80f ,ForceMode.Force);
            }
        }
        if (grounded)
        {
            Rigidbody.AddForce(direction.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            Rigidbody.AddForce(direction.normalized * moveSpeed * 10f* airMultiplier, ForceMode.Force);
        }
        if(!wallRunning) Rigidbody.useGravity=!OnSlope();
    }
    private void SpeedControl()
    {
        if (OnSlope()&&!exitingSlope)
        {
            if (Rigidbody.velocity.magnitude>moveSpeed)
            {
                Rigidbody.velocity = Rigidbody.velocity.normalized * moveSpeed;
            }
        }
        Vector3 flatVel = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z);

        if (flatVel.magnitude>moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            Rigidbody.velocity = new Vector3(limitedVel.x, Rigidbody.velocity.y, limitedVel.z);
        }
    }
    private void Jump()
    {
        exitingSlope = true;
        Rigidbody.velocity = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z);

        Rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope=false;
    }
    private void CrouchPoint()
    {
        flag = Physics.Raycast(crouchingPoint.transform.position, Vector3.up, playerHeight * 0.5f, Physics.AllLayers);
        Debug.Log(flag);
        Debug.DrawRay(crouchingPoint.transform.position, Vector3.up, Color.red ,1, true);
        
    }
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f+ 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < masSlopeAngle && angle !=0;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
