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
        private MoveState state;
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
        private bool cancelMoveCallback;
        private AudioSource audioSource;
        internal bool isBlocking;
        internal bool collisionOccured;

        public enum MoveState
        {
            Idle,
            Knockback,
            Sliding,
            Move,
            Busy,
            Slerp
        }

        private void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            rigidbody = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();
            selectionCircle = GameObject.FindGameObjectWithTag("SelectionCircle");
            state = MoveState.Idle;
        }

        private void Update()
        {
            switch (state)
            {
                case MoveState.Idle:
                    break;
                case MoveState.Sliding:

                    if (rigidbody.velocity.magnitude < 0.01f)
                    {
                        state = MoveState.Idle;
                        onMoveComplete();
                    }

                    break;
                case MoveState.Busy:
                    break;
                case MoveState.Move:
                    var sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    float moveSpeed = 4f;
                    rigidbody.velocity = (slideTargetPosition - transform.position).normalized * moveSpeed;

                    if (sqrMag > lastSqrMag)
                    {
                        rigidbody.velocity = Vector3.zero;
                        state = MoveState.Idle;
                        if (!cancelMoveCallback)
                            onMoveComplete();
                        else return;
                    }

                    lastSqrMag = sqrMag;
                    break;
                case MoveState.Knockback:
                    break;
                case MoveState.Slerp:
                    sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    moveSpeed = 4f;
                    rigidbody.transform.Translate (Vector3.Slerp(rigidbody.transform.position, slideTargetPosition, 0.1f));

                    if (sqrMag > lastSqrMag)
                    {
                        rigidbody.velocity = Vector3.zero;
                        state = MoveState.Idle;
                        if (!cancelMoveCallback)
                            onMoveComplete();
                        else return;
                    }

                    lastSqrMag = sqrMag;
                    break;
            }
        }

        public void UseSkill(BaseSkill skill, Action onSkillComplete)
        {
            if (skill.TotalUses != 0)
                skill.remainingUses -= 1;

            UIManager.GetInstance().SetTextThenFade($"{this.GetBaseActor().Name} uses {skill.Name}!", 0.5f);

            if (skill.TargetOption == TARGETINGOPTION.ENEMY)
            {
                if (isControllable()) // Player attacking enemy
                {
                    InputManager.GetInstance().WaitForTarget(() => PerformSkillOnTarget(skill, onSkillComplete));
                }
                else // Enemy attacking player
                {
                    var targetActor = BattleManager.GetInstance().PlayerActors[0];
                    UIManager.GetInstance().DrawAllActorReactionsAboveSpeed(targetActor, skill.Speed, () => PerformSkillOnTarget(skill, onSkillComplete));
                }
            }
            if (skill.TargetOption == TARGETINGOPTION.SELF)
            {
                PerformSkillOnSelf(skill, onSkillComplete);
            }
        }

        private void PerformSkillOnSelf(BaseSkill skill, Action onSkillComplete)
        {
            skill.WarmUp(this, null, () =>
            {
                UIManager.GetInstance().AddToStoneSlab($"{skill.Name}");
                skill.Perform(this, skill, onSkillComplete);
            });
        }

        private void PerformSkillOnTarget(BaseSkill skill, Action onSkillComplete)
        {
            var targetActor = isControllable() ? InputManager.GetInstance().GetSelectedTarget() : BattleManager.GetInstance().PlayerActors[0];


            skill.WarmUp(this, targetActor, () =>
            {
                var targetReaction = isControllable() ? targetActor.ChooseValidReactionAtRandom(skill.Speed) : UIManager.GetInstance().GetSelectedReaction();
                UIManager.GetInstance().AddToStoneSlab($"{skill.Name} vs {targetReaction.Name}");

                targetReaction.WarmUp(targetActor, () =>
                {
                    skill.Perform(this, targetActor, targetReaction, () =>
                    {
                        targetReaction.React(targetActor, () =>
                        {
                            // Apply Damage
                            var damage = targetReaction.DamageModifier(skill.Damage + this.GetBaseActor().ATK);
                            targetActor.GetBaseActor().DealDamage(damage);
                            targetActor.PlayAudio("Blow1");

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
                this.MoveToPosition(transform.position + direction.normalized * force, MoveState.Sliding, () => { animator.Play("Idle"); onKnockbackFinished(); });

            }
            else
                onKnockbackFinished();

        }

        public void MoveToPosition(Vector3 TargetPosition, MoveState state, Action onMoveComplete)
        {
            if (state == MoveState.Sliding)
                rigidbody.velocity = (TargetPosition - transform.position);

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = TargetPosition;
            this.onMoveComplete = onMoveComplete;
            this.state = state;
        }

        public void Move(Vector3 direction, Action onMoveComplete)
        {
            UIManager.GetInstance().AddToStoneSlab($"{baseActor.Name} moves");
            PlayAudio("Move2");

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = transform.position + (direction * baseActor.AGI);
            this.onMoveComplete = onMoveComplete;
            this.state = MoveState.Move;

        }

        public void Move(Vector2 direction, Action onMoveComplete)
        {
            UIManager.GetInstance().AddToStoneSlab($"{baseActor.Name} moves");
            PlayAudio("Move2");

            var camera = Camera.main;
            var forward = camera.transform.forward; forward.y = 0;
            var right = camera.transform.right; right.y = 0;
            forward.Normalize(); right.Normalize();

            var desiredMoveDirection = forward * direction.y + right * direction.x;
            Move(desiredMoveDirection, onMoveComplete);
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

        public BaseReaction ChooseValidReactionAtRandom(int minimumReactionSpeed)
        {
            List<BaseReaction> validreactions = new List<BaseReaction>();

            validreactions = baseActor.reactions.Where(reaction => reaction.Speed >= minimumReactionSpeed && reaction.HasUsesLeft()).ToList();
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

        public void PlayAudio(string clipName)
        {
            var clip = SoundManager.instance.GetAudioClipByName(clipName);
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void ShowSelectionCircle()
        {
            selectionCircle.GetComponent<SelectionCircle>().target = this.transform;
        }

        public void AnimationHit()
        {
            onAnimationHitComplete();
        }

        public void ResetFlags()
        {
            isBlocking = false;
            collisionOccured = false;
        }

        public bool isControllable()
        {
            return baseActor.Controllable;
        }

        public Actor GetBaseActor()
        {
            return baseActor;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var velocityThreshold = 1;

            // Check if collided object has the "Level" tag
            if (collisionOccured) return;

            if (collision.gameObject.CompareTag("Level"))
            {
                Debug.Log($"Velocity on collision is {rigidbody.velocity.magnitude / Time.deltaTime}");

                UIManager.GetInstance().AddToStoneSlab($"{baseActor.Name} hits a wall!");
                baseActor.DealPostureDamage(5f);
                collisionOccured = true;

                // Check if velocity magnitude is greater than the threshold
                if (rigidbody.velocity.magnitude / Time.deltaTime > 0.00001)
                {
                    // Calculate mirrored velocity (mirror along current velocity)
                    Vector3 mirroredVelocity = Vector3.Reflect(rigidbody.velocity, collision.contacts[0].normal);

                    // Replace current velocity with the mirrored velocity
                    rigidbody.velocity = mirroredVelocity;
                }
            }
        }
    }
}
