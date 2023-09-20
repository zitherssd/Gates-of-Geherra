using Assets.Scripts.Actions;
using Assets.Scripts.Utility;
using System;
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
                    //float slideSpeed = 2f;
                    //transform.position += (slideTargetPosition - transform.position) * slideSpeed * Time.deltaTime;

                    //float reachedDistance = 0.2f;
                    //if (Vector3.Distance(transform.position, slideTargetPosition) < reachedDistance)
                    //{
                    //    state = State.Idle;
                    //    onMoveComplete();
                    //}
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
                    //transform.position += (slideTargetPosition - transform.position).normalized * moveSpeed * Time.deltaTime;
                    Debug.Log((slideTargetPosition - transform.position).normalized * moveSpeed * Time.deltaTime);
                    rigidbody.velocity = (slideTargetPosition - transform.position).normalized * moveSpeed;

                    if (sqrMag > lastSqrMag)
                    {
                        rigidbody.velocity = Vector3.zero;
                        state = State.Idle;
                        onMoveComplete();
                    }

                    lastSqrMag = sqrMag;

                    //float error = 0.01f;
                    //if (Vector3.Distance(transform.position, slideTargetPosition) < error)
                    //{
                    //    state = State.Idle;
                    //    onMoveComplete();
                    //}
                    break;
            }
        }

        public void UseSkill(BaseSkill skill, Action onSkillComplete)
        {
            if (skill.TotalUses != 0)
                skill.remainingUses -= 1;
            if (skill.TargetOption == TARGETINGOPTION.ENEMY)
            {
                if (isControllable()) //Player attacking enemy
                {
                    InputManager.GetInstance().WaitForTarget(() =>
                    {
                        var targetActor = InputManager.GetInstance().GetSelectedTarget();
                        {
                            skill.WarmUp(this, () =>
                            {
                                //Get reaction
                                var targetReaction = targetActor.ChooseReactionAtRandom();
                                targetReaction.WarmUp(targetActor, () =>
                                {
                                    skill.Perform(this, targetActor, skill, targetReaction, () =>
                                    {
                                        targetReaction.React(targetActor, () =>
                                        {
                                            //Apply Damage
                                            var damage = targetReaction.DamageModifier(skill.CalculateDamage()); //Calculate damage after reaction
                                            targetActor.GetBaseActor().DealDamage(damage); 

                                            //Apply Knockback
                                            var direction = targetActor.transform.position - this.transform.position;
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
                    });
                }
                else //Enemy attacking player
                {
                    var targetActor = BattleManager.GetInstance().PlayerActors[0];
                    {
                        skill.WarmUp(this, () =>
                        {
                            //Get reaction
                            UIManager.GetInstance().DrawAllActorReactions(targetActor, () =>
                            {
                                var targetReaction = UIManager.GetInstance().GetSelectedReaction();
                                targetReaction.WarmUp(targetActor, () =>
                                {
                                    skill.Perform(this, targetActor, skill, targetReaction, () =>
                                    {
                                        targetReaction.React(targetActor, () =>
                                        {
                                            //Apply Damage
                                            var damage = targetReaction.DamageModifier(skill.CalculateDamage()); //Calculate damage after reaction
                                            targetActor.GetBaseActor().DealDamage(damage);

                                            //Apply Knockback
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
                        });
                    }
                }
            }
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

        private void ApplyDamageWithKnockback(Vector3 direction, float damage, Action onKnockbackFinished)
        {
            var actoranimator = this.GetComponent<Animator>();
            if (damage > 0)
            {
                if (!isBlocking)
                    if (direction.y > 0.1f)
                        actoranimator.Play("HurtAir");
                    else
                        actoranimator.Play("HurtGround");

                baseActor.DealDamage(damage);
                this.MoveToPosition(transform.position + direction * damage, State.Sliding, () => { actoranimator.Play("Idle"); onKnockbackFinished(); });
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

        public void MoveToPosition(Vector2 direction, Action onMoveComplete)
        {
            var target = direction.normalized * baseActor.AGI;
            var camera = Camera.main;
            var forward = camera.transform.forward; forward.y = 0;
            var right = camera.transform.right; right.y = 0;
            forward.Normalize(); right.Normalize();

            var desiredMoveDirection = forward * target.y + right * target.x;
            Debug.Log(desiredMoveDirection);
            MoveToPosition(desiredMoveDirection * baseActor.AGI + transform.position, State.Move, () =>
             {
                 state = State.Idle;
                 onMoveComplete();
             });
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

        public BaseReaction ChooseReactionAtRandom()
        {
            if (baseActor.reactions.Count == 0) return new BaseReaction { ReactionType = BaseReaction.REACTIONTYPE.NONE };
            var random = new System.Random();
            var index = random.Next(baseActor.reactions.Count);

            return baseActor.reactions[index];
        }

        public BaseSkill ChooseSkillAtRandom()
        {
            var random = new System.Random();
            var index = random.Next(baseActor.skills.Count);

            return baseActor.skills[index];
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
