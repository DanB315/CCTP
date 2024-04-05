using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [Header("SENSITIVITY VALUES")]
    public float horizontalSens;
    public float verticalSens;
    public float mouseLookSpeedX;
    public float mouseLookSpeedY;
    public float controllerLookSpeedX;
    public float controllerLookSpeedY;

    


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
                if (playerControls.Camera.Look.activeControl.device.name == "Mouse")
                {
                    horizontalSens = mouseLookSpeedY;
                    verticalSens = mouseLookSpeedX;
                }

                else
                {
                    horizontalSens = controllerLookSpeedY;
                    verticalSens = controllerLookSpeedX;
                }
                float mouseX = lookInput.x * horizontalSens;
                float mouseY = lookInput.y * verticalSens;

                verticalRotation += mouseX;

                horizontalRotation -= mouseY;
                horizontalRotation = Mathf.Clamp(horizontalRotation, -90f, 90f);

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
