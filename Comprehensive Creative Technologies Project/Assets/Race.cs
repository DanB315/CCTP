using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Race : MonoBehaviour
{
    public IncreaseTime it;
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
            }
        

    }

}
