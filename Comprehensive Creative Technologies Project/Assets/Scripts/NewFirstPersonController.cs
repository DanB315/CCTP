using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NewFirstPersonController : MonoBehaviour
{
    [Header("MOVEMENT")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallRunSpeed;
    public float dashSpeed;
    public bool canMove = true;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMulti;
    public float slopeIncreaseMulti;

    public float groundDrag;

    [Header("IS PLAYER")]
    public bool isCrouching;
    public bool isSprinting;
    public bool isSliding;
    public bool isWallrunning;
    public bool isDashing;

    bool keepMomentum;

    [Header("HEADBOB VALUES")]
    public float walkBobSpeed;
    public float walkBobAmount;
    public float crouchBobSpeed;
    public float crouchBobAmount;
    public float sprintBobSpeed;
    public float sprintBobAmount;
    private float defaultYPos = 0;
    private float timer;

    [Header("JUMPING VALUES")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMulti;
    bool readyToJump;
    public float mayJump;

    [Header("CROUCHING VALUES")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    private InputAction jumpInput;
    private InputAction sprintInput;
    private InputAction crouchInput;

    [Header("GROUNDED")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("SLOPE VALUES")]
    public float maxSlopeAngle;
    public RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("REFERENCES")]
    public Climbing climbing;
    public Camera playerCam;
    public Transform camHolder;
    public Transform orientation;

    float hInput;
    float vInput;
    PlayerControls playerControls;
    public  Vector2 currentInput = Vector2.zero;

    Vector3 moveDir;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        dashing,
        sliding,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
        defaultYPos = camHolder.transform.localPosition.y;
    }

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        jumpInput = playerControls.Movement.Jump;
        sprintInput = playerControls.Movement.Sprint;
        crouchInput = playerControls.Movement.Crouch;
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        if (grounded)
        {
            mayJump = 0.15f;
        }

        if (mayJump < 0)
        {
            mayJump = 0;
        }

        mayJump -= Time.deltaTime;
        
        MyInput();
        SpeedControl();
        StateMachine();
        if (!isSliding)
        {
            HandleHeadbob();
        }
        
        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
        {
            rb.drag = groundDrag;
        }

        else
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        currentInput = playerControls.Movement.Move.ReadValue<Vector2>();
        hInput = currentInput.x;
        vInput = currentInput.y;

        if (jumpInput.IsPressed() && readyToJump && mayJump > 0f)
        {
            mayJump = 0;
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (crouchInput.WasPressedThisFrame() && grounded && !isSprinting)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (!crouchInput.IsPressed())
        {
            if (isCrouching && Physics.Raycast(playerCam.transform.position, Vector3.up, 1f))
            {
                return;
            }

            else
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }
    }

    private void StateMachine()
    {
        if (isDashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
        }

        else if (isWallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }

        else if (isSliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
            }

            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }

        else if (crouchInput.IsPressed() && grounded && !isSprinting)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
            isSprinting = false;
            isCrouching = true;
        }

        else if (grounded && sprintInput.IsPressed())
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
            isSprinting = true;
            isCrouching = false;
        }

        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
            isSprinting = false;
        }

        else
        {
            state = MovementState.air;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }

            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }

        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f)
        {
            keepMomentum = false;
        }
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMulti * slopeIncreaseMulti * slopeAngleIncrease;
            }

            else
            {
                time += Time.deltaTime * speedIncreaseMulti;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (climbing.exitingWall)
        {
            return;
        }

        moveDir = orientation.forward * vInput + orientation.right * hInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDir(moveDir) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        if (grounded)
        {
            rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        
        else if (!grounded)
        {
            rb.AddForce(moveDir.normalized * moveSpeed * 10f * airMulti, ForceMode.Force);
        }

        if (!isWallrunning)
        {
            rb.useGravity = !OnSlope();
        }
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }

        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void HandleHeadbob()
    {
        if (!grounded)
        {
            return;
        }

        if (Mathf.Abs(moveDir.x) > 0.1f || Mathf.Abs(moveDir.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
            camHolder.transform.localPosition = new Vector3(
                camHolder.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount),
                camHolder.transform.localPosition.z);
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDir(Vector3 dir)
    {
        return Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;
    }
}
