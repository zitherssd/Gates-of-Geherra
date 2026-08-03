using Unity.VisualScripting;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine;

namespace Assets.Scripts.Battle.Actor.States
{
    public interface IAttack
    {
        public void OnEnd();
        public void OnHit();
        public void EnterWindup(Animator animator)
        {
        }
        public void EnterRecovery(Animator animator)
        {
        }
    }
}