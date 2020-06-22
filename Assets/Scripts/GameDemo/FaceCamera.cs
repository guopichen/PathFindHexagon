using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform targetCamera;

    private Transform m_transform;
    void Start()
    {
        targetCamera = GameObject.Find("Main Camera").transform;
        m_transform = this.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if(targetCamera)
        {
            transform.LookAt(targetCamera);
            this.enabled = false;
        }
    }
}
