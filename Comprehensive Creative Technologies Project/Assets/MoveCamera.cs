using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPos;
    public Transform camera;

    private void Update()
    {
        camera.position = cameraPos.position;
    }
}
