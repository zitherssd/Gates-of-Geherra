using Assets.Scripts.Actions;
using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions.Skills
{
    [CreateAssetMenu(fileName = "Teleport", menuName = "ScriptableObjects/Skills/Teleport")]
    public class Teleport : BaseSkill
    {
        public override void Perform(BaseActorBattler casterActor, Action onPerformEnd)
        {
            casterActor.PlayAnimation("Ninjutsu", () =>
            {
                casterActor.transform.position = casterActor.transform.position + GetRelativeToCamera(StickValue) * 10f;
                onPerformEnd();
            }, null);
            return;
        }
    }
}
