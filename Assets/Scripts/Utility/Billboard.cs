using Assets.Scripts.Battle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private BillboardType billboardType;

    private Quaternion shurikenRotation = Quaternion.Euler(50f, 0, 0);
    private new Camera camera;
    private GameObject graphicObj;
    private float prevrotation;
    private Actor actor;

    public void Start()
    {
        camera = Camera.main;
        if (billboardType == BillboardType.Shuriken) return;
        graphicObj = transform.GetChild(0).gameObject;
        if(gameObject.name == "Billboard")
        actor = gameObject.GetComponentInParent<Actor>();
    }

    public enum BillboardType { LookAtCamera, CameraForward,
        Shuriken
    }

    private void Update()
    {
        UpdateOrientation();

    }

    private void UpdateOrientation()
    {
        if (actor != null)
        {
            Vector3 camerRight = Camera.main.transform.right;
            float dotProduct = Vector3.Dot(actor.transform.forward, camerRight.normalized);
            if (dotProduct > 0f)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void LateUpdate()
    {
        switch(billboardType)
        {
            case BillboardType.LookAtCamera:
                transform.LookAt(camera.transform.position, Vector3.up);
                break;
            case BillboardType.CameraForward:
                transform.forward = camera.transform.forward;
                break;
            case BillboardType.Shuriken:
                // Rotate the object around its own up axis (Y-axis) over time
                float rotationSpeed = 30f; // Adjust this value as needed
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

                // Align the object with the camera's forward direction
                transform.forward = camera.transform.forward;
                transform.rotation *= shurikenRotation;
                break;
            default:
                break;
        }
    }
}
