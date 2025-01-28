using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Utility
{
    public class CanvasHandler : MonoBehaviour
    {
        public static CanvasHandler instance = null;

        public GameObject EndButton;
        public GameObject ActionSlot;

        void Start ()
        {
            if (instance == null) instance = this;
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