using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dashing : MonoBehaviour
{
    [Header("Ref")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private NewFirstPersonController fpc;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardsForce;
    public float dashDuration;
    private Vector3 delayedForceToApply;

    [Header("Cooldown")]
    public float dashCool;
    private float dashCoolTimer;

    [Header("Input")]
    InputAction dashInput;

    PlayerControls playerControls;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        dashInput = playerControls.Movement.Dash;
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        fpc = GetComponent<NewFirstPersonController>();
    }

    private void Update()
    {
        if (dashInput.IsPressed())
        {
            Dash();
        }

        if (dashCoolTimer > 0)
        {
            dashCoolTimer -= Time.deltaTime;
        }
    }

    private void Dash()
    {
        if (dashCoolTimer > 0)
        {
            return;
        }

        else
        {
            dashCoolTimer = dashCool;
        } 
            
        fpc.isDashing = true;

        Vector3 forceToApply = orientation.forward * dashForce + orientation.up * dashUpwardsForce;

        rb.AddForce(forceToApply, ForceMode.Impulse);

        Invoke(nameof(DelayedDashForce), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce()
    {
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        fpc.isDashing = false;
        dashCoolTimer = dashCool;
    }
}
