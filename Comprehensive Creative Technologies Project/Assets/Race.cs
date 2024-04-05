using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Race : MonoBehaviour
{
    public IncreaseTime it;

    [Header("RACE VARIABLES")]
    public bool levelOneDone = false;
    public string levelOneTime;
    public bool levelTwoDone = false;
    public string levelTwoTime;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "StartPoint")
        {
            it.time = 0;
            it.timerOn = true;
            print("START");
        }
            
        if (other.tag == "EndPoint")
        {

            it.timerOn = false;
            print("END");
            if (!levelOneDone)
            {
                levelOneDone = true;
                levelOneTime = it.timer.text;
            }

            else
            {
                levelTwoDone = true;
                levelTwoTime = it.timer.text;
            }
        }
    }

}
