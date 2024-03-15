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


    [Header("Sens Values")]
    public float sensX;
    public float sensY;
    public float mouseLookSpeedX;
    public float mouseLookSpeedY;
    public float controllerLookSpeedX;
    public float controllerLookSpeedY;

    [Header("References")]
    public Climbing climbingScript;


    float xRot;
    float yRot;

    Quaternion startRot;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        startRot = Quaternion.Euler(0, 0, 0);
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
        if (!climbingScript.climbing)
        {
            lookInput = playerControls.Camera.Look.ReadValue<Vector2>();
            if (playerControls.Camera.Look.activeControl.device.name == "Mouse")
            {
                sensX = mouseLookSpeedY;
                sensY = mouseLookSpeedX;
            }

            else
            {
                sensX = controllerLookSpeedY;
                sensY = controllerLookSpeedX;
            }
            float mouseX = lookInput.x * sensX;
            float mouseY = lookInput.y * sensY;

            yRot += mouseX;

            xRot -= mouseY;
            xRot = Mathf.Clamp(xRot, -90f, 90f);

            CamHolder.rotation = Quaternion.Euler(xRot, yRot, 0);
            orientation.rotation = Quaternion.Euler(0, yRot, 0);
        }
    }

    
    public void DoFov(float endVal)
    {
        GetComponent<Camera>().DOFieldOfView(endVal, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
    
}
