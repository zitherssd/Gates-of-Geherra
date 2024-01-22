using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ActionSlot : MonoBehaviour, IDropHandler
{
    public GameObject skill { get
        {
            if(transform.childCount > 0)
            {
                return transform.GetChild(0).gameObject;
            }
            return null;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("SOMETHIND DROPPED");
        if (!skill)
        {
            GameObject dropped = eventData.pointerDrag;
            ButtonHandler draggableItem = dropped.GetComponent<ButtonHandler>();
            draggableItem.parentafterDrag = transform;
            ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());

            if(draggableItem.referencedAction != null)
            {
                if (draggableItem.referencedAction.Tags.Contains(Assets.TAG.USESLIDER))
                    UIManager.GetInstance().Slider.SetActive(true);
                if (draggableItem.referencedAction.Tags.Contains(Assets.TAG.USEKNOB))
                    UIManager.GetInstance().Knob.SetActive(true);
            }
            if (draggableItem.referencedReaction != null)
            {
                if (draggableItem.referencedReaction.Tags.Contains(Assets.TAG.USESLIDER))
                    UIManager.GetInstance().Slider.SetActive(true);
                if (draggableItem.referencedReaction.Tags.Contains(Assets.TAG.USEKNOB))
                    UIManager.GetInstance().Knob.SetActive(true);
            }
        }
    }
}
