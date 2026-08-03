using UnityEngine;

namespace Assets.Scripts.Core
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
