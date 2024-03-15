using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    FirstPersonController firstPersonController;

    public Vector2 movementInput;
    public Vector2 cameraInput;

    public float cameraInputX;
    public float cameraInputY;

    public float moveAmount;
    public float verticalInput;
    public float horizontalInput;

    public bool shiftInput;

    public bool controlInput;

    public bool jumpInput;

    public bool qInput;

    public bool attackInput;

    public bool lockOnInput;


    private void Awake()
    {
        firstPersonController = GetComponent<FirstPersonController>();
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.Movement.Move.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.Camera.Look.performed += i => cameraInput = i.ReadValue<Vector2>();

            playerControls.Movement.Sprint.performed += i => shiftInput = !shiftInput;  

            playerControls.Movement.Crouch.performed += i => controlInput = true;
            playerControls.Movement.Crouch.canceled += i => controlInput = false;

            playerControls.Movement.Jump.performed += i => jumpInput = true;

            //playerControls.PlayerActions.Q.performed += i => qInput = true;

            //playerControls.PlayerActions.Attack.performed += i => attackInput = true;
        }

        playerControls.Enable();

    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMovementInput();
        HandleSprintingInput();
        HandleWalkingInput();
        HandleJumpingInput();
        //HandleDodgeInput();
        //HandleAttackInput();
    }

    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputY = cameraInput.y;
        cameraInputX = cameraInput.x;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        if (moveAmount == 0)
        {
            shiftInput = false;
        }

        if (controlInput)
        {
            shiftInput = false;
        }
    }

    private void HandleSprintingInput()
    {
        if (shiftInput && moveAmount > 0.5f)
        {
            firstPersonController.isSprinting = true;
        }

        else
        {
            firstPersonController.isSprinting = false;
        }
    }

    private void HandleWalkingInput()
    {
        if (controlInput)
        {
            firstPersonController.isWalking = true;
        }

        else
        {
            firstPersonController.isWalking = false;
        }
    }

    private void HandleJumpingInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            //firstPersonController.HandleJump();
        }
    }

    /*
    private void HandleDodgeInput()
    {
        if (qInput)
        {
            qInput = false;
            playerLocomotion.HandleDodge();
        }
    }
    

    private void HandleAttackInput()
    {
        if (attackInput)
        {
            attackInput = false;
            firstPersonController.HandleAttack();
        }
    }
    */
}
