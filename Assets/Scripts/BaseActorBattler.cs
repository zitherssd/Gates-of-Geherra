using Assets.Scripts.Actions;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    public class BaseActorBattler : MonoBehaviour
    {
        [SerializeField]
        private Actor baseActor;
        private State state;
        private bool isPlayerTeam;
        private GameObject selectionCircle;
        private Vector3 slideTargetPosition;
        private Action onMoveComplete;
        private Action onAnimationHitComplete;
        private Animator animator;
        private InputManager inputManager;
        private Rigidbody rigidbody;
        private CapsuleCollider capsuleCollider;
        private float lastSqrMag;
        internal bool isBlocking;

        public enum State
        {
            Idle,
            Sliding,
            Move,
            Busy
        }

        private void Awake()
        {
            rigidbody = gameObject.GetComponentInParent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();
            selectionCircle = GameObject.FindGameObjectWithTag("SelectionCircle");
            state = State.Idle;
        }

        private void Update()
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Sliding:

                    if (rigidbody.velocity.magnitude < 0.01f)
                    {
                        state = State.Idle;
                        onMoveComplete();
                    }

                    break;
                case State.Busy:
                    break;
                case State.Move:
                    var sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    float moveSpeed = 4f;
                    Debug.Log((slideTargetPosition - transform.position).normalized * moveSpeed * Time.deltaTime);
                    rigidbody.velocity = (slideTargetPosition - transform.position).normalized * moveSpeed;

                    if (sqrMag > lastSqrMag)
                    {
                        rigidbody.velocity = Vector3.zero;
                        state = State.Idle;
                        onMoveComplete();
                    }

                    lastSqrMag = sqrMag;
                    break;
            }
        }

        public void UseSkill(BaseSkill skill, Action onSkillComplete)
        {
            if (skill.TotalUses != 0)
                skill.remainingUses -= 1;

            if (skill.TargetOption == TARGETINGOPTION.ENEMY)
            {
                if (isControllable()) // Player attacking enemy
                {
                    InputManager.GetInstance().WaitForTarget(() => PerformSkillOnTarget(skill, onSkillComplete));
                }
                else // Enemy attacking player
                {
                    var targetActor = BattleManager.GetInstance().PlayerActors[0];
                    UIManager.GetInstance().DrawAllActorReactions(targetActor, () => PerformSkillOnTarget(skill, onSkillComplete));
                }
            }
            if (skill.TargetOption == TARGETINGOPTION.SELF)
            {
                PerformSkillOnSelf(skill, onSkillComplete);
            }
        }

        private void PerformSkillOnSelf(BaseSkill skill, Action onSkillComplete)
        {
            skill.WarmUp(this, () =>
            {
                UIManager.GetInstance().AddToStoneSlab($"{skill.Name}");
                skill.Perform(this, skill, onSkillComplete);
            });
        }

        private void PerformSkillOnTarget(BaseSkill skill, Action onSkillComplete)
        {
            var targetActor = isControllable() ? InputManager.GetInstance().GetSelectedTarget() : BattleManager.GetInstance().PlayerActors[0];

            skill.WarmUp(this, () =>
            {
                var targetReaction = isControllable() ? targetActor.ChooseValidReactionAtRandom() : UIManager.GetInstance().GetSelectedReaction();
                UIManager.GetInstance().AddToStoneSlab($"{skill.Name} vs {targetReaction.Name}");

                targetReaction.WarmUp(targetActor, () =>
                {
                    skill.Perform(this, targetActor, skill, targetReaction, () =>
                    {
                        targetReaction.React(targetActor, () =>
                        {
                            // Apply Damage
                            var damage = targetReaction.DamageModifier(skill.Damage + this.GetBaseActor().ATK);
                            targetActor.GetBaseActor().DealDamage(damage);

                            // Appply Posture
                            var postureDamage = targetReaction.PostureModifier(skill.PostureDamage);
                            var broken = targetActor.GetBaseActor().DealPostureDamage(postureDamage);
                            if (broken)
                            {
                                UIManager.GetInstance().AddToStoneSlab("Posture break!");
                                BattleManager.GetInstance().RepeatTurn(); //Posture break check
                            }


                                // Apply Knockback
                                var direction = (targetActor.transform.position - this.transform.position).normalized;
                            if (skill.Tags.Contains(SKILLTAG.KNOCKBACK_AIR)) direction = (direction + Vector3.up).normalized;
                            targetActor.ApplyKnockback(direction, targetReaction.KnockbackModifier(skill.KnockbackForce), () =>
                            {
                                onSkillComplete();
                            });
                        });
                    });
                });
            });
        }

        private void ApplyKnockback(Vector3 direction, float force, Action onKnockbackFinished)
        {
            var animator = this.GetComponent<Animator>();

            if (force > 0)
            {
                //Animation
                if (!isBlocking)
                    if (direction.y > 0.1)
                        animator.Play("HurtAir");
                    else
                        animator.Play("HurtGround");

                //Force
                this.MoveToPosition(transform.position + direction.normalized * force, State.Sliding, () => { animator.Play("Idle"); onKnockbackFinished(); });

            }
            else
                onKnockbackFinished();

        }

        public void MoveToPosition(Vector3 TargetPosition, State state, Action onMoveComplete)
        {
            if (state == State.Sliding)
                rigidbody.velocity = (TargetPosition - transform.position);

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = TargetPosition;
            this.onMoveComplete = onMoveComplete;
            this.state = state;
        }

        public void Move(Vector3 direction, Action onMoveComplete)
        {
            UIManager.GetInstance().AddToStoneSlab($"{baseActor.Name} moves");

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = transform.position + (direction * baseActor.AGI);
            this.onMoveComplete = onMoveComplete;
            this.state = State.Move;
        }

        public void PlayAnimation(string AnimationName, Action onAnimationHitComplete)
        {
            animator.Play(AnimationName);
            this.onAnimationHitComplete = onAnimationHitComplete;
        }
        public void PlayAnimation(string AnimationName)
        {
            animator.Play(AnimationName);
        }

        public BaseReaction ChooseValidReactionAtRandom()
        {
            List<BaseReaction> validreactions = new List<BaseReaction>();

            validreactions = baseActor.reactions.Where(reaction => reaction.HasUsesLeft()).ToList();
            if (validreactions.Count == 0) return new BaseReaction { ReactionType = BaseReaction.REACTIONTYPE.NONE, Name = "Nothing" };

            var random = new System.Random();
            var index = random.Next(validreactions.Count);

            return validreactions[index];
        }

        public BaseSkill ChooseValidSkillAtRandom()
        {
            List<BaseSkill> validskills = new List<BaseSkill>();

            validskills = baseActor.skills.Where(skills => skills.HasUsesLeft()).ToList();

            if (validskills.Count == 0) return new BaseSkill { TargetOption = TARGETINGOPTION.SELF, Name = "Nothing" };

            var random = new System.Random();
            var index = random.Next(validskills.Count);

            return validskills[index];
        }

        public BaseSkill ChooseValidSkillAtRandomOrReturnNull()
        {
            List<BaseSkill> validskills = new List<BaseSkill>();

            validskills = baseActor.skills.Where(skills => skills.IsValid(this)).ToList();

            if (validskills.Count == 0) return null;

            var random = new System.Random();
            var index = random.Next(validskills.Count);

            return validskills[index];
        }

        public void ShowSelectionCircle()
        {
            selectionCircle.GetComponent<SelectionCircle>().target = this.transform;
        }

        public void AnimationHit()
        {
            onAnimationHitComplete();
        }

        public bool isControllable()
        {
            return baseActor.Controllable;
        }

        public Actor GetBaseActor()
        {
            return baseActor;
        }
    }
}
