// Assets/Scripts/UI/DropSlot.cs

using UnityEngine;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    

public class DropSlot : MonoBehaviour, IDropHandler
{
    // These should be configured in the Inspector for each container
    public string containerID;
    public int maxItems = 999;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        ActionButtonInventory dragScript = droppedObject.GetComponent<ActionButtonInventory>();
        if (dragScript == null) return;

        // Check if the container is full.
        // If we are dropping back into the same container, the count is acceptable.
        if (transform.childCount >= maxItems && dragScript.originalParent != this.transform)
        {
            Debug.Log(containerID + " container is full!");
            return; // Don't allow drop
        }

        // Parent the object to this slot
        droppedObject.transform.SetParent(this.transform);

        // Find the correct sibling index to insert the item for reordering
        int newSiblingIndex = transform.childCount - 1;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (droppedObject.transform.position.x < transform.GetChild(i).position.x)
            {
                newSiblingIndex = i;
                break;
            }
        }
        droppedObject.transform.SetSiblingIndex(newSiblingIndex);
    }
}
}
