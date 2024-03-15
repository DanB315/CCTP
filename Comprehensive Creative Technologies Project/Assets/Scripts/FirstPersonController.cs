using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    public PlayerControls playerControls;
    private InputAction move;
    private InputAction jump;
    private InputAction crouch;
    private InputAction look;
    private InputAction zoom;

    public bool canMove = true;
    public bool isWalking;
    public bool isSprinting;
    public bool isCrouching;
    public bool restricted = false;
    public bool freeze = false;
    private bool shouldJump => characterController.isGrounded;
    private bool shouldCrouch => !duringCrouchAnimation && characterController.isGrounded;

    Transform CameraObject;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool willSlideOnSlopes = true;
    [SerializeField] private bool canZoom = true;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;

    [Header("Look Parameters")]
    [SerializeField, Range(0, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(0, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] public float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool duringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.025f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.05f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.01f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Zoom Parameters")]
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;

    private Vector3 hitPointNormal;


    private bool IsSliding
    {
        get
        {
            if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }


    private Camera playerCam;
    private CharacterController characterController;

    private Vector3 moveDir;
    public Vector2 currentInput = Vector2.zero;

    private Vector2 rotationInput = Vector2.zero;

    private float rotationX = 0;


    void Start()
    {
        playerCam = GetComponentInChildren<Camera>();
        CameraObject = Camera.main.transform;
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCam.transform.localPosition.y;
        defaultFOV = playerCam.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        move = playerControls.Movement.Move;
        jump = playerControls.Movement.Jump;
        crouch = playerControls.Movement.Crouch;
        look = playerControls.Camera.Look;
        zoom = playerControls.Camera.Zoom;

        move.Enable();
        jump.Enable();
        crouch.Enable();
        look.Enable();
        zoom.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
        move.Disable();
    }
    void Update()
    {
        HandleMouseLook();
        if (canMove)
        {
            HandleMovementInput();
            

            if (canSprint)
            {
                playerControls.Movement.Sprint.performed += x => SprintPressed();
                //playerControls.Movement.Sprint.canceled += x => SprintReleased();
            }

            if (canJump)
            {
                jump.performed += HandleJump;
            }

            if (canCrouch)
            {
                crouch.performed += HandleCrouch;
            }

            if (canUseHeadbob)
            {
                HandleHeadbob();
            }

        }

        if (canZoom)
        {
            zoom.started += HandleZoom;
            zoom.canceled += CancelZoom;
        }

         if (restricted == false)
         {
             ApplyFinalMovements();
         }
    }

    private void HandleMovementInput()
    {
        //currentInput = new Vector2((isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));
        currentInput = (isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkSpeed) * move.ReadValue<Vector2>();
        if (currentInput.x == 0 && currentInput.y == 0)
        {
            isSprinting = false;
            
        }

        float moveDirY = moveDir.y;
        moveDir = (transform.TransformDirection(Vector3.forward) * currentInput.y) + (transform.TransformDirection(Vector3.right) * currentInput.x);
        moveDir.y = moveDirY;
    }

    private void SprintPressed()
    {
        if (characterController.isGrounded)
        {
            isSprinting = !isSprinting;
        }
    }

    private void HandleJump(InputAction.CallbackContext context)
    {
        if (shouldJump)
        {
            moveDir.y = jumpForce;
        }
    }

    private void HandleCrouch(InputAction.CallbackContext context)
    {
        if (shouldCrouch)
        {
            StartCoroutine(CrouchStand());
            isSprinting = false;
        }
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded)
        {
            return;
        }

        if (Mathf.Abs(moveDir.x) > 0.1f || Mathf.Abs(moveDir.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : isSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCam.transform.localPosition = new Vector3(
                playerCam.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : isSprinting ? sprintBobAmount : walkBobAmount),
                playerCam.transform.localPosition.z);
        }
    }

    private void HandleZoom(InputAction.CallbackContext context)
    {
        if (zoomRoutine != null)
        {
            StopCoroutine(zoomRoutine);
            zoomRoutine = null;
        }

        zoomRoutine = StartCoroutine(ToggleZoom(true));

        /*
        if (context.canceled)
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(false));
        }
        */
    }

    private void CancelZoom(InputAction.CallbackContext context)
    {
        zoomRoutine = StartCoroutine(ToggleZoom(false));
    }

    private void HandleMouseLook()
    {
        rotationInput = look.ReadValue<Vector2>();
        rotationX -= rotationInput.y * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, rotationInput.x * lookSpeedX, 0);
    }

    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
        {
            moveDir.y -= gravity * Time.deltaTime;
        }

        if (willSlideOnSlopes && IsSliding)
        {
            moveDir += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }

        characterController.Move(moveDir * Time.deltaTime);
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCam.transform.position, Vector3.up, 1f))
        {
            yield break;
        }

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    private IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startingFOV = playerCam.fieldOfView;
        float timeElapsed = 0;

        while (timeElapsed < timeToZoom)
        {
            playerCam.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed / timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCam.fieldOfView = targetFOV;
        zoomRoutine = null;
    }
}
