using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObject;
    private Rigidbody rb;
    private NewFirstPersonController fpc;
    public PlayerCamera playerCam;

    [Header("Sliding")]
    public float maxSlideTime = 1f;
    public float slideForce = 40f;
    private float slideTimer;

    public float slideYScale = 0.5f;
    private float startYScale;

    private float hInput;
    private float vInput;
    Vector2 currentInput = Vector2.zero;

    PlayerControls playerControls;
    InputAction slide;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        slide = playerControls.Movement.Slide;
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        fpc = GetComponent<NewFirstPersonController>();

        orientation = GameObject.Find("Orientation").transform;
        playerObject = GameObject.Find("PlayerObject").transform;
        playerCam = Camera.main.GetComponent<PlayerCamera>();


        startYScale = playerObject.localScale.y;
    }
    private void Update()
    {
        currentInput = playerControls.Movement.Move.ReadValue<Vector2>();
        hInput = currentInput.x;
        vInput = currentInput.y;

        if (slide.WasPressedThisFrame() && fpc.isSprinting && fpc.grounded)
        {
            StartSlide();
        }

        else if (slide.WasReleasedThisFrame() && fpc.isSliding)
        {
            StopSlide();
        }

    }

    private void FixedUpdate()
    {
        if (fpc.isSliding)
        {
            SlidingMovement();
        }
    }
    private void StartSlide()
    {
        fpc.isSliding = true;

        playerObject.localScale = new Vector3(playerObject.localScale.x, slideYScale, playerObject.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;

        playerCam.DoFov(100f);
    }

    private void SlidingMovement()
    {
        Vector3 inputDir = orientation.forward * vInput + orientation.right * hInput;

        if (!fpc.OnSlope() || rb.velocity.y > -1f)
        {
            rb.AddForce(inputDir.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        else
        {
            rb.AddForce(fpc.GetSlopeMoveDir(inputDir) * slideForce, ForceMode.Force);
        }


        if (slideTimer <= 0)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        fpc.isSliding = false;

        playerObject.localScale = new Vector3(playerObject.localScale.x, startYScale, playerObject.localScale.z);

        playerCam.DoFov(90f);
    }
}
