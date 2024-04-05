using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Pause : MonoBehaviour
{
    [Header("REFERENCES")]
    public GameObject pauseCanvas;
    private GameObject player;
    private NewFirstPersonController fpc;
    private Climbing climbing;
    private Teleport teleport;
    private WallRunning wallRun;
    private Dashing dashing;
    private Sliding sliding;
    IncreaseTime timer;
    Race race;
    ResetPlayer reset;

    [Header("PAUSED")]
    public bool isPaused = false;
    public TextMeshProUGUI jumpText;
    public TextMeshProUGUI slidingText;
    public TextMeshProUGUI dashingText;
    public TextMeshProUGUI climbingText;
    public TextMeshProUGUI wallrunText;
    public TextMeshProUGUI teleportText;
    public TextMeshProUGUI level1Time;
    public TextMeshProUGUI level2Time;
    public TextMeshProUGUI deathCounter;

    PlayerControls playerControls;

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

    private void Start()
    {
        pauseCanvas.SetActive(false);
        player = GameObject.Find("Player");
        fpc = player.GetComponent<NewFirstPersonController>();
        climbing = player.GetComponent<Climbing>();
        teleport = player.GetComponent<Teleport>();
        wallRun = player.GetComponent<WallRunning>();
        dashing = player.GetComponent<Dashing>();
        sliding = player.GetComponent<Sliding>();
        race = player.GetComponent<Race>();
        reset = player.GetComponent<ResetPlayer>();
    }

    private void Update()
    {
        if (playerControls.Actions.Pause.WasPressedThisFrame())
        {        
            HandlePause();
        }

        jumpText.text = fpc.jumpcounter.ToString();
        slidingText.text = sliding.slideCounter.ToString();
        dashingText.text = dashing.dashCounter.ToString();
        climbingText.text = climbing.climbCounter.ToString();
        wallrunText.text = wallRun.wallRunCounter.ToString();
        teleportText.text = teleport.teleportCounter.ToString();

        level1Time.text = race.levelOneTime;
        level2Time.text = race.levelTwoTime;

        deathCounter.text = reset.deaths.ToString();
    }

    private void HandlePause()
    {
        if (!isPaused)
        {
            print("PAUSE");
            isPaused = true;
            Time.timeScale = 0f;
            pauseCanvas.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        else if (isPaused)
        {
            print("PLAY");
            isPaused = false;
            Time.timeScale = 1f;
            pauseCanvas.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ExitGame()
    {
        print("QUIT");
        Application.Quit();
    }
}
