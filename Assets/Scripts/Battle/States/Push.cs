using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle.States
{
    public class Push : BaseStatus
    {
        public Push(Actor owner) : base(owner)
        {

        }

        public override void Apply()
        {
            if (owner.activeStates.OfType<Push>().Any())
            {
                owner.activeStates.Remove(this);
            }
        }
        public override void Remove()
        {
            base.Remove();
        }
    }
}