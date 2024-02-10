using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Utility
{
    public class CanvasHandler : MonoBehaviour, IHasChanged
    {
        public GameObject EndButton;
        public GameObject ActionSlot;

        void Start ()
        {
            HasChanged();
        }

        public void HasChanged()
        {
            if (ActionSlot.transform.childCount > 0)
                EndButton.GetComponentInChildren<TextMeshProUGUI>().text = "ACT";
            else
                EndButton.GetComponentInChildren<TextMeshProUGUI>().text = "SKIP";

        }
    }
}

namespace UnityEngine.EventSystems
{
    public interface IHasChanged : IEventSystemHandler
    {
        void HasChanged();
    }
}