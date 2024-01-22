using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "Move", menuName = "ScriptableObjects/Action/Move", order = 1)]
    public class Move : BaseAction
    {
        public override void Perform(BaseActorBattler casterActor, Action onPerformEnd)
        {
            if (casterActor.isControllable())
            {
                InputManager.instance.WaitForTargetActor(targetActor =>
                {
                    var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;
                    casterActor.rigidbody.AddForce(casterToTarget * 100 * 6);
                    casterActor.PlayAnimation("Run", () =>
                    {
                        onPerformEnd();
                    });
                });

            }
            else
            {
                var targetActor = BattleManager.GetInstance().PlayerActors[0];
                var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;
                casterActor.rigidbody.AddForce(casterToTarget * 100 * 6);
                casterActor.PlayAnimation("Run", () =>
                {
                    onPerformEnd();
                });
            }
        }
    }
}