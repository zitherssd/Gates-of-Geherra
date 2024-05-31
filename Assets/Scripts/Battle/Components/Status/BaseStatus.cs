using UnityEngine;

namespace Assets.Scripts.Battle.Components.Status
{
    public abstract class BaseStatus
    {
        public bool singleInstance = false;
        public Actor owner;

        public virtual void Apply() { }
        public virtual void Remove() { }
        public virtual void Tick() { }
    }
}