using UnityEngine;

namespace Assets.Scripts.Utility
{
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

    }
}
