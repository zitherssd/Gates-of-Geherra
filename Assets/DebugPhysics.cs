using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPhysics : MonoBehaviour
{
    public float force;
    private Rigidbody rigidbody;

    private void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                // Get the point of intersection
                Vector3 hitPoint = hit.point;
                Debug.Log(hitPoint);
                rigidbody.AddForce((hit.point - transform.position).normalized * force * 100);
            }
        }
    }
}
