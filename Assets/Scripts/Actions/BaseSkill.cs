using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Assets.BaseActorBattler;

namespace Assets.Scripts.Actions
{
    [CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/Skill", order = 1)]
    public class BaseSkill : BaseAction
    {

        public ANIMATIONTYPE AnimationType;
        public ATTACKTYPE AttackType;
        public TARGETINGOPTION TargetOption;
        public List<SKILLTAG> Tags;
        public float Power;
        public float KnockbackForce;



        public void Perform(BaseActorBattler casterActor, BaseActorBattler enemyActor, BaseSkill skill, BaseReaction reaction, Action onPerformEnd)
        {
            //ONLY FOR TARGETINGOPTION.ENEMY SKILLS
            if (skill.Tags.Contains(SKILLTAG.MOVE_NEAR_ENEMY_BEFORE_ATTACK))
            {
                var targetVector = enemyActor.transform.position - casterActor.transform.position;

                if (!(targetVector.magnitude < 2f))
                {
                    var minimumDistance = targetVector.normalized;
                    casterActor.MoveToPosition(casterActor.transform.position + targetVector - minimumDistance, State.Move, () => {
                        casterActor.PlayAnimation(skill.AnimationType.ToString(), () => 
                        {
                            onPerformEnd();
                        });
                    });
                }
                else casterActor.PlayAnimation(skill.AnimationType.ToString(), () =>
                {
                    onPerformEnd();
                });
            };
        }

        public float CalculateDamage()
        {
            switch (AttackType)
            {
                case ATTACKTYPE.Attack:
                    return Power;
                case ATTACKTYPE.Super:
                    return Power * 2;
                case ATTACKTYPE.Bla:
                    return Power * 3;
            }
            return 0;
        }

        public void WarmUp(BaseActorBattler casterActor, Action onWarmUpEnd)
        {
            var texttobeshown = $"{casterActor.name} is using { this.Name }!";
            UIManager.GetInstance().SetTextThenFade(texttobeshown, 1f);
            onWarmUpEnd();
            //Play an animation before moving,
            //Show skill name,
            //Certain special effects
        }
    }

    public enum ANIMATIONTYPE { NONE, Punch, Kick, Shuriken, Highkick }
    public enum ATTACKTYPE { Attack, Super, Bla }
    public enum TARGETINGOPTION { SELF, ENEMY }
    public enum SKILLTAG { MOVE_NEAR_ENEMY_BEFORE_ATTACK, PROJECTILE, KNOCKBACK_AIR }


}