using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    public class StatusManager : MonoBehaviour
    {
        private Actor.Actor owner;
        public List<BaseStatus> activeStatuses; 

        public StatusManager(Actor.Actor owner)
        {
            this.owner = owner;
            activeStatuses = new List<BaseStatus>();
        }

        public void Add(BaseStatus status)
        {
           // if (status.singleInstance == true && activeStatuses.OfType<Stagger>().Any()) return;

            activeStatuses.Add(status);
            status.owner = owner;
            status.Apply();
 
        }
        public void Tick()
        {
            foreach(var status in activeStatuses)
            {
                status.Tick();
            }
        }

        public void Remove(BaseStatus status)
        {
            activeStatuses.Remove(status);
            status.Remove();
        }
    }
}