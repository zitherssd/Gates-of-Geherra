using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private BillboardType billboardType;

    private Quaternion shurikenRotation = Quaternion.Euler(50f, 0, 0);
    private new Camera camera;
    private GameObject thisgameobj;
    private float prevrotation;

    public void Start()
    {
        camera = Camera.main;
        thisgameobj = transform.gameObject;
    }

    public enum BillboardType { LookAtCamera, CameraForward,
        Shuriken
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
