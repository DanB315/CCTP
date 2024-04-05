using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public LayerMask whatIsWall;
    public LayerMask whatIsNotWall;
    public NewFirstPersonController fpc;
    public Transform cam;

    PlayerControls playerControls;
    Vector2 currentInput;

    [Header("Climbing")]
    public float climbSpeed;
    public float maxClimbTime;
    private float climbTimer;
    public float climbCounter = 0f;

    public bool climbing = false;

    [Header("Climb Jump")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    InputAction jumpInput;
    public int climbJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLegth;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallInFront;
    private bool notClimbable;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall = false;
    public float exitWallTime;
    private float exitWallTimer;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        jumpInput = playerControls.Movement.Jump;
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
    private void Update()
    {
        currentInput = playerControls.Movement.Move.ReadValue<Vector2>();

        WallCheck();
        StateMachine();

        if (climbing && !exitingWall)
        {
            ClimbingMovment();
        }
    }

    private void StateMachine()
    {

        if (wallInFront && currentInput.y >= 1 && wallLookAngle < maxWallLookAngle && !exitingWall && !notClimbable && !fpc.isWallrunning)
        {
            if (!climbing && climbTimer > 0)
            {
                StartClmbing();
            }

            if (climbTimer > 0)
            {
                climbTimer -= Time.deltaTime;
            }

            if (climbTimer < 0)
            {
                StopClimbing();
            }
        }

        else if (exitingWall)
        {
            if (climbing)
            {
                StopClimbing();
            }

            if (exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }

            if (exitWallTimer < 0)
            {
                exitingWall = false;
            }
        }

        else
        {
            if (climbing)
            {
                StopClimbing();
            }
        }

        if (wallInFront && jumpInput.WasPressedThisFrame() && climbJumpsLeft > 0)
        {
            ClimbJump();
        }
               
    }
    private void WallCheck()
    {
        wallInFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLegth);
        notClimbable = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLegth, whatIsNotWall);
        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallInFront && newWall) || fpc.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    private void StartClmbing()
    {
        climbing = true;
        climbCounter++;
        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    private void ClimbingMovment()
    {
        rb.velocity = new Vector3(0, climbSpeed, 0);
    }

    private void StopClimbing()
    {
        climbing = false;
    }

    private void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        //cam.rotation = Quaternion.Euler(0, 180, 0);

        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
