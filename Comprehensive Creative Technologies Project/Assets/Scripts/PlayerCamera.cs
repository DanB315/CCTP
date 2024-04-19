using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    public PlayerControls playerControls;


    private Vector2 lookInput = Vector2.zero;

    public Transform orientation;
    public Transform CamHolder;

    [Header("REFRENCES")]
    public Climbing climbingScript;
    public Camera playerCam;
    public Pause pause;
    public PlayerInput playerInput;

    [Header("SENSITIVITY VALUES")]
    public float horizontalSens;
    public float verticalSens;
    public float mouseLookSpeedX;
    public float mouseLookSpeedY;
    public float controllerLookSpeedX;
    public float controllerLookSpeedY;
    float camX;
    float camY;

    


    float horizontalRotation;
    float verticalRotation;

    Quaternion startRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        startRot = Quaternion.Euler(0, 0, 0);
        playerCam = GameObject.Find("Camera").GetComponent<Camera>();
        pause = GameObject.Find("LevelManager").GetComponent<Pause>();
        playerInput = GameObject.Find("LevelManager").GetComponent<PlayerInput>();
    }

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        if (!pause.isPaused)
        {
            if (!climbingScript.climbing)
            {
                lookInput = playerControls.Camera.Look.ReadValue<Vector2>();
                if (playerInput.currentControlScheme == "Keyboard & Mouse")
                {
                    horizontalSens = mouseLookSpeedY;
                    verticalSens = mouseLookSpeedX;
                    camX = lookInput.x * horizontalSens;
                    camY = lookInput.y * verticalSens;
                }

                else if (playerInput.currentControlScheme == "Gamepad")
                {
                    horizontalSens = controllerLookSpeedY;
                    verticalSens = controllerLookSpeedX;
                    camX = lookInput.x * horizontalSens * Time.deltaTime;
                    camY = lookInput.y * verticalSens * Time.deltaTime;
                }

                else
                {
                    print("NO INPUT SYSTEM DETECTED");

                }

                verticalRotation += camX;

                horizontalRotation -= camY;
                horizontalRotation = Mathf.Clamp(horizontalRotation, -80f, 80f);

                CamHolder.rotation = Quaternion.Euler(horizontalRotation, verticalRotation, 0);
                orientation.rotation = Quaternion.Euler(0, verticalRotation, 0);
            }
        }
        
    }

    
    public void ChangeFieldOfView(float endVal)
    {
        playerCam.DOFieldOfView(endVal, 0.25f);
    }

    public void ChangeCamRotation(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
    
}
