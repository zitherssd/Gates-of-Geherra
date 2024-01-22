using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragMove : MonoBehaviour
{

        private Vector3 offset;
        private Vector3 initMouse;
        private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnMouseUp();
        }

        if (isDragging)
        {
            OnMouseDrag();
        }
    }

    void OnMouseDown()
        {
        Debug.Log("Enter OnMouseDown");
            // Calculate the offset between the mouse click point and the object's position
            initMouse = Input.mousePosition;
            offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        void OnMouseDrag()
        {
            if (isDragging)
            {
                // Get the current mouse position in world coordinates
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // Update the object's position to follow the mouse drag
                //transform.position = new Vector3(mousePosition.x + offset.x, mousePosition.y + offset.y, transform.position.z);
            }
        }

        void OnMouseUp()
        {
        Debug.Log("Enter OnMouseUp");
        if (isDragging)
            {
                var newpos = Input.mousePosition - initMouse;
                Debug.Log(newpos);
                newpos = newpos.normalized;
                GetComponent<Rigidbody>().AddForce(newpos * 30);
            }
            isDragging = false;
        }

}
