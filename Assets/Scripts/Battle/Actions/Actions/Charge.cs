using System;
using UnityEngine;

namespace Assets.Scripts.Battle.Actions
{
    [CreateAssetMenu(fileName = "Charge", menuName = "ScriptableObjects/Action/Charge", order = 1)]
    public class Charge : BaseAction
    {
        public override void Perform(BaseActorBattler casterActor, Action onPerformEnd)
        {
            if (casterActor.isControllable())
            {
                InputManager.instance.WaitForTargetActor(targetActor =>
                {
                    var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;
                    casterActor.GetComponent<Rigidbody>().AddForce(casterToTarget * 100 * 4);
                    casterActor.PlayAnimation("Run", () =>
                    {
                        onPerformEnd();
                    });
                });

            }
            else
            {
                var targetActor = BattleManager.instance.PlayerActors[0];
                var casterToTarget = (targetActor.transform.position - casterActor.transform.position).normalized;
                casterActor.GetComponent<Rigidbody>().AddForce(casterToTarget * 100 * 4);
                casterActor.PlayAnimation("Run", () =>
                {
                    onPerformEnd();
                });
            }
        }
    }
}