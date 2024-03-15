using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IncreaseTime : MonoBehaviour
{
    public TMP_Text timer;
    public float time = 0;

    public bool timerOn = false;
    // Update is called once per frame

    void Update()
    {
        if (timerOn)
        {
            time += Time.deltaTime;
        }
        updateTimer(time);
    }

    void updateTimer(float currentTime)
    {
        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

        timer.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }
}
