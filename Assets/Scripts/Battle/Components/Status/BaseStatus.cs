using UnityEngine;

namespace Assets.Scripts.Battle.Components.Status
{
    public abstract class BaseStatus : ScriptableObject
    {
        public bool singleInstance = false;
        [System.NonSerialized]
        public Actor.Actor owner;

        public virtual void Apply() { }
        public virtual void Remove() { }
        public virtual void Tick() { }
    }
}