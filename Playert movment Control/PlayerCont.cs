using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCont : NetworkBehaviour
{
    [SerializeField] private Transform spawnObjectTest;
    [Header("Movement")]
    public float moveSpeed;           // Oyuncunun mevcut hareket hızı
    public float walkSpeed;           // Oyuncunun yürüme hızı
    public float sprintSpeed;         // Oyuncunun sprint hızı
    public float sliderSpeed;         // Oyuncu kayarken kullanılan hız
    public float climbingSpeed;       // Tırmanma sırasında kullanılan hız
    public float wallRunSpeed;        // Duvar koşusu sırasında kullanılan hız
    private float desiredMoveSpeed;   // Hedef hareket hızı
    [SerializeField]private float lastDesiredMoveSpeed; // Önceki hedef hız, hız değişimlerini yumuşatmak için kullanılır

    public float groundDrag;          // Yerdeyken uygulanan sürtünme kuvveti

    [Header("Crouching")]
    public bool flag;
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    public GameObject crouchingPoint;

    [Header("Slope Handling")]
    public float masSlopeAngle;       // Tırmanılabilecek maksimum eğim açısı
    private RaycastHit slopeHit;      // Eğim yüzeyi için çarpma bilgisi
    private bool exitingSlope;        // Eğimi terk ederken durum

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool readyToJump;
    [Header("References")]
    public Climbing climbingScript;
    public PlayerAnimatorController animController;
    public PlayerCam playerCam; // PlayerCam bileşenine referans ekleyin

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

    public enum MovementState
    {
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

    [SerializeField] float horizontalInput;
    [SerializeField] float verticalInput;

    [SerializeField] Vector3 direction;
    Rigidbody Rigidbody;

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.freezeRotation = true;
        climbingScript = GetComponent<Climbing>();

        readyToJump = true;
        startYScale = transform.localScale.y;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        animController = GetComponent<PlayerAnimatorController>();

    }

    private NetworkVariable<int> index = new NetworkVariable<int>(1);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) 
        {
            Transform spawnedObject=Instantiate(spawnObjectTest);
            spawnedObject.GetComponent<NetworkObject>().Spawn(true);
        }
        if(!IsOwner) return;
        CrouchPoint();
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        StateHandler();
        SpeedControl();
        animController.setTurnsFloat(playerCam.GetXNormalized());
        transform.rotation = Quaternion.Euler(0, playerCam.GetYRotation(), 0);
        if (grounded)
        {
            Rigidbody.linearDamping = groundDrag;
        }
        else
        {
            Rigidbody.linearDamping = 0;
        }

        

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 1.2f);
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
            desiredMoveSpeed = OnSlope() && Rigidbody.linearVelocity.y < 0.1f ? sliderSpeed : sprintSpeed;

        }
        //Hızlı Kosma
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
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 3f && moveSpeed != 0)
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

        while (time < diffrence)
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
        animController.setAnimasyonWalkingSpeed(moveSpeed, horizontalInput);

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
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
        if (Input.GetKeyUp(crouchKey) && !flag)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        }

    }
    public void MovePlayer()
    {
        direction = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (OnSlope() && !exitingSlope)
        {
            Rigidbody.AddForce(GetSlopeMoveDirection(direction) * moveSpeed * 20f, ForceMode.Force);
            if (Rigidbody.linearVelocity.y > 0)
            {
                Rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        if (grounded)
        {
            Rigidbody.AddForce(direction.normalized * moveSpeed * 10f, ForceMode.Force);
            if (Rigidbody.linearVelocity.magnitude > 3 && verticalInput!=0)
            {
                animController.setBoolMovement();
            }
            else
            {
                animController.setBoolMovementFalse();
            }
        }
        else if (!grounded)
        {
            animController.setBoolMovementFalse();
            Rigidbody.AddForce(direction.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
        if (!wallRunning) Rigidbody.useGravity = !OnSlope();
    }
    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (Rigidbody.linearVelocity.magnitude > moveSpeed)
            {
                Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * moveSpeed;
                
            }
        }
        Vector3 flatVel = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            Rigidbody.linearVelocity = new Vector3(limitedVel.x, Rigidbody.linearVelocity.y, limitedVel.z);
        }
    }
    private void Jump()
    {
        exitingSlope = true;
        Rigidbody.linearVelocity = new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z);
        animController.setTrigger("Jumps");
        Rigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }
    private void CrouchPoint()
    {
        flag = Physics.Raycast(crouchingPoint.transform.position, Vector3.up, playerHeight * 0.5f, Physics.AllLayers);
        Debug.DrawRay(crouchingPoint.transform.position, Vector3.down, Color.red, 1, true);

    }
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < masSlopeAngle && angle != 0;
        }
        return false;
    }
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
