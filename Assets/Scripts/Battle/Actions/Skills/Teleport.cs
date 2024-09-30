using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "Teleport", menuName = "ScriptableObjects/Skills/Teleport")]
    public class Teleport : BaseSkill
    {
        public ANIMATION type;
        public float Distance;
        protected override void PerformSpecific(Actor casterActor, Action onPerformEnd)
        {
            casterActor.PlayAnimation(type.ToString(), () =>
            {
                CameraManager.instance.SlowTrack = true;
                casterActor.transform.position = casterActor.transform.position + Direction * Distance;
            }, onPerformEnd);
        }

        private void Awake()
        {
            StickMult = Distance;
        }

        public enum ANIMATION { Ninjutsu, ShadowStep }
    }
}
