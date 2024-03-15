using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Teleport : MonoBehaviour
{
    [Header("References")]
    PlayerControls playerControls;
    public Transform orientation;
    Rigidbody rb;
    PlayerCamera playerCam;
    Camera cam;
    NewFirstPersonController fpc;
    Image crossHair;

    [Header("Teleport Variables")]
    public float teleportDis;
    public float teleportCooldown;
    private float timer;
    private float cooldown;
    public LayerMask whatIsTeleport;
    public bool canTeleport = true;
    public bool teleportActive = false;


    InputAction teleportInput;
    public GameObject cube;
    Collider cubeCol;
    Vector3 cubeSize;
    Vector3 colliderNormal;
    Rigidbody cubeRb;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
        teleportInput = playerControls.Movement.Teleport;
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        //Ray myRay = new Ray(transform.position, orientation.forward);
        cube.SetActive(false);
        cubeCol = cube.GetComponent<Collider>();
        cubeSize = cubeCol.bounds.size;
        rb = GetComponent<Rigidbody>();
        timer = teleportCooldown;
        playerCam = Camera.main.GetComponent<PlayerCamera>();
        cam = Camera.main;
        fpc = GameObject.Find("Player").GetComponent<NewFirstPersonController>();
        crossHair = GameObject.Find("CrossHair").GetComponent<Image>();
    }

    private void Update()
    {
        Ray myRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        if (timer <= 0)
        {
            canTeleport = true;
        }

        RaycastHit hitData;

        if (canTeleport)
        {
            crossHair.color = Color.cyan; 
        }

        else if (!canTeleport)
        {
            crossHair.color = Color.red;
        }

        if (teleportInput.IsPressed())
        {
            if (canTeleport)
            {
                cube.SetActive(true);
                playerCam.DoFov(110);
                teleportActive = true;
                if (!fpc.grounded)
                {
                    Time.timeScale = 0.5f;
                }

                else
                {
                    Time.timeScale = 1f;
                }
                if (Physics.Raycast(cam.transform.position, orientation.forward, out hitData, teleportDis, ~whatIsTeleport))
                {
                    colliderNormal = hitData.normal.normalized;
                    if (colliderNormal.x < 0 || colliderNormal.y < 0 || colliderNormal.z < 0)
                    {
                        cube.transform.position = new Vector3(hitData.point.x - cube.transform.localScale.x * 0.5f, hitData.point.y - cube.transform.localScale.y * 0.5f, hitData.point.z - cube.transform.localScale.z * 0.5f);
                    }

                    else
                    {
                        cube.transform.position = new Vector3(hitData.point.x + cube.transform.localScale.x * 0.5f, hitData.point.y + cube.transform.localScale.y * 0.5f, hitData.point.z + cube.transform.localScale.z * 0.5f);
                    }
                }

                else
                {
                    cube.transform.position = cam.transform.position + orientation.forward * teleportDis;
                }
            }

        }


        if (teleportInput.WasReleasedThisFrame())
        {
            if (canTeleport && teleportActive)
            {
                ActivateTeleport();
            }
        }
    }
    private void ActivateTeleport()
    {
        playerCam.DoFov(70);
        rb.MovePosition(cube.transform.position);
        StartCoroutine(resetFov(0.1f));
        timer = teleportCooldown;
        canTeleport = false;
        teleportActive = false;
        cube.SetActive(false);
        Time.timeScale = 1f;
    }

    IEnumerator resetFov(float secs)
    {
        yield return new WaitForSeconds(secs);
        playerCam.DoFov(90);
    }
}
