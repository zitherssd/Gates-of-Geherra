using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "Teleport", menuName = "ScriptableObjects/Skills/Teleport")]
    public class Teleport : BaseSkill
    {
        public float Distance;
        private Actor caster;
        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            caster = casterActor;
            casterActor.state.TransitionTo(casterActor.state.actingState.Set(this, onPerformEnd));
        }

        private void Awake()
        {
            StickMult = Distance;
        }

        public override void OnHit()
        {
            CameraManager.instance.SlowTrack = true;
            caster.transform.position = caster.transform.position + Direction * Distance;
            caster.movement.FaceTarget(caster.target.ClosestEnemy);

        }
    }
}
