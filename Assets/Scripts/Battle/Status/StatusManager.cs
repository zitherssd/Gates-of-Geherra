using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.Status
{
    public class StatusManager : MonoBehaviour
    {
        private Actor.Actor owner;
        public List<BaseStatus> activeStatuses;

        private void Awake()
        {
            owner = GetComponent<Actor.Actor>();
            if (activeStatuses == null) activeStatuses = new List<BaseStatus>();
        }

        public void Add(BaseStatus status)
        {
            if (status == null) return;

            // singleInstance: replace an existing status of the same type instead of stacking.
            if (status.singleInstance)
            {
                var existing = activeStatuses.FirstOrDefault(s => s != null && s.GetType() == status.GetType());
                if (existing != null)
                {
                    activeStatuses.Remove(existing);
                    existing.Remove();
                }
            }

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
            if (status == null || !activeStatuses.Contains(status)) return;
            activeStatuses.Remove(status);
            status.Remove();
        }

        public bool HasStatus<T>() where T : BaseStatus => activeStatuses.Any(s => s is T);

        public T GetStatus<T>() where T : BaseStatus => activeStatuses.OfType<T>().FirstOrDefault();
    }
}