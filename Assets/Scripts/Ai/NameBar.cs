using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameBar : MonoBehaviour
{
    private Camera myCamera;

    private void Start()
    {
        myCamera = Camera.main;
    }

    private void LateUpdate()
    {
        transform.LookAt(myCamera.transform);
        transform.Rotate(0, 180, 0);
    }
}
