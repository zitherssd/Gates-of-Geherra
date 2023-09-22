using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class SelectionCircle : MonoBehaviour
    {
        public Transform target;

        // Update is called once per frame
        void Update()
        {
            if(target != null)
            transform.position = new Vector3(target.transform.position.x, 0.01f, target.transform.position.z);
        }
    }
}