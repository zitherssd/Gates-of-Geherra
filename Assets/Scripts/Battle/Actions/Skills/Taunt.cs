using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "Taunt", menuName = "ScriptableObjects/Skills/Taunt")]
    public class Taunt : BaseSkill
    {
        protected override void PerformSpecific(BaseActorBattler casterActor, Action onPerformEnd)
        {
            casterActor.PlayAnimation("Taunt", () =>
            {
                casterActor.GetComponentInChildren<ParticleSystem>().Play(); //to b ereplace
                casterActor.transform.position = casterActor.transform.position + GetRelativeToCamera(StickValue) * 10f;
                casterActor.GetComponentInChildren<ParticleSystem>().Play();
                onPerformEnd();
            }, onPerformEnd);
            return;
        }
    }
}
