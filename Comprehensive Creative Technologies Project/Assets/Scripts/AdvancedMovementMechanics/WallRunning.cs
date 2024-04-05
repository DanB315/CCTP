using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    private float wallRunTimer;
    public float wallClimbSpeed;
    public float wallRunCounter = 0f;

    private float hInput;
    private float vInput;

    PlayerControls playerControls;
    Vector2 currentInput;

    InputAction runUpInput;
    public bool upwardsRunning;

    InputAction jumpInput;

    private Quaternion startRot;
    private Quaternion current;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exit")]
    private bool exitWall;
    public float exitWallTime;
    public float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform oritentation;
    private NewFirstPersonController fpc;
    public PlayerCamera camera;
    private Rigidbody rb;
    public Climbing climbing;
    private GameObject camHolder;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        jumpInput = playerControls.Movement.Jump;
        runUpInput = playerControls.Movement.Jump;
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        fpc = GetComponent<NewFirstPersonController>();
        camHolder = GameObject.Find("CameraHolder");
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (fpc.isWallrunning)
        {
            WallRunMovement();
        }
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, oritentation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -oritentation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        currentInput = playerControls.Movement.Move.ReadValue<Vector2>();
        hInput = currentInput.x;
        vInput = currentInput.y;

        upwardsRunning = runUpInput.IsPressed() || (camHolder.transform.eulerAngles.x <= 350 && camHolder.transform.eulerAngles.x >= 270);


        if ((wallLeft || wallRight) && vInput > 0 && AboveGround() && !exitWall && !climbing.climbing)
        {
            if (!fpc.isWallrunning)
            {
                StartWallRun();
            }

            if (wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }

            if (wallRunTimer <= 0 && fpc.isWallrunning)
            {
                exitWall = true;
                exitWallTimer = exitWallTime;
            }

            if (jumpInput.WasPressedThisFrame())
            {
                WallJump();
            }
        }

        else if (exitWall)
        {
            if (fpc.isWallrunning)
            {
                StopWallRun();
            }

            if (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            if (exitWallTimer <= 0)
            {
                exitWall = false;
            }
        }

        else
        {
            if (fpc.isWallrunning)
            {
                StopWallRun();
            }
        }
    }

    private void StartWallRun()
    {
        fpc.isWallrunning = true;
        wallRunCounter++;

        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (wallRight)
        {
            camera.ChangeCamRotation(10f);
        }

        else if (wallLeft)
        {
            camera.ChangeCamRotation(-10f);
        }

        camera.ChangeFieldOfView(100f);
    }

    private void WallRunMovement()
    {
        rb.useGravity = useGravity;        

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((oritentation.forward - wallForward).magnitude > (oritentation.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (upwardsRunning)
        {
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        }

        if(!(wallLeft && hInput > 0) && !(wallRight && vInput > 0))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }

        if (useGravity)
        {
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
        
    }

    private void StopWallRun()
    {
        fpc.isWallrunning = false;

        camera.ChangeCamRotation(0f);
        camera.ChangeFieldOfView(90f);
    }    

    private void WallJump()
    {
        exitWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.velocity = new Vector3(rb.velocity.z, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
