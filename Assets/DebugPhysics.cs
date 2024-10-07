using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPhysics : MonoBehaviour
{
    public float force;
    private Rigidbody rigidbody;
    private Collider collider;
    private PhysicMaterial pmaterial;

    private void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
        collider = gameObject.GetComponent<Collider>();
        pmaterial = collider.material;
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
                // Get the point of intersectio
                pmaterial.dynamicFriction = 1f;
                Vector3 hitPoint = hit.point;
                Debug.Log(hitPoint);
                rigidbody.AddForce((hit.point - transform.position).normalized * force, ForceMode.Impulse);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            // Create a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
                pmaterial.dynamicFriction = 0f;
                Vector3 hitPoint = hit.point;
                Debug.Log(hitPoint);
                rigidbody.AddForce((hit.point - transform.position).normalized * force, ForceMode.Impulse);
            }
        }
    }
}
