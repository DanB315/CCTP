using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlayer : MonoBehaviour
{
    private GameObject[] startPoint;
    private GameObject[] exitPoint;
    private GameObject player;
    private GameObject currPoint;
    int i = 0;
    public float deaths = 0f;

    private void Start()
    {
        player = GameObject.Find("Player");
        startPoint = GameObject.FindGameObjectsWithTag("StartPoint");
        exitPoint = GameObject.FindGameObjectsWithTag("EndPoint");
        currPoint = startPoint[i];
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Death")
        {
            player.transform.position = currPoint.transform.position;
            deaths++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EndPoint")
        {
            currPoint = startPoint[i + 1];
            player.transform.position = currPoint.transform.position;
        }
    }
}
