using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Assets.BaseAction;

public class ActionSlot : MonoBehaviour
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

    public void OnDrop()
    {
        if (!skill)
        {
            UIManager.GetInstance().Slider.SetActive(false);
            UIManager.GetInstance().Knob.SetActive(false);
        }
        else
        {
            GameObject dropped = skill.gameObject;
            var handler = skill.GetComponent<ButtonHandler>();
            if (handler.referencedAction != null)
            {
                if (handler.referencedAction.Tags.Contains(TAG.USESTICK))
                    UIManager.GetInstance().Knob.SetActive(true);
                else
                    UIManager.GetInstance().Knob.SetActive(false);

            }
        }
        ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());

    }
}
