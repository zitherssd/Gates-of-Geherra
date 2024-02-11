using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.States;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets
{
    public class BaseActorBattler : MonoBehaviour
    {
        [SerializeField] public Actor Actor;

        // Movement and Animation
        private MoveState currentState;
        private Vector3 slideTargetPosition;
        private Action onMoveComplete;
        private Action onAnimationHitComplete;
        private Action onAnimationEndComplete;
        private Action onReactionCheck;
        private Animator animator;
        private new Rigidbody rigidbody;
        private float lastSqrMag;
        // Audio
        private AudioSource audioSource;
        // Collision
        internal bool collisionOccured;
        // Prefabs and Objects
        public GameObject damagePopupPrefab;
        public GameObject posturePopupPrefab;
        public LayerMask layer;
        // States and Actions
        public List<BaseStatus> activeStates;
        public delegate float DamageModifier(float damage);
        public delegate float PostureModifier(float damage);
        public delegate float KnockbackModifier(float force, Vector3 direction);
        public event DamageModifier ApplyDamageModifiers;
        public event PostureModifier ApplyPostureModifiers;
        public event KnockbackModifier ApplyKnockbackModifiers;
        // UI
        public OriginPointHandler originPointInUI;
        public GameObject selectionCircle;


        private void Awake()
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            rigidbody = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();
            selectionCircle = GameObject.FindGameObjectWithTag("SelectionCircle");
            currentState = MoveState.Idle;
            activeStates = new List<BaseStatus>();

            //var cc = GetComponentInChildren<ColorController>();
            //cc.mainColor = baseActor.mainColor;
            //cc.secondaryColor = baseActor.secondaryColor;
        }
        private void FixedUpdate()
        {
            if (IsGrounded()) activeStates.RemoveAll(state => state is Midair);
            switch (currentState)
            {
                case MoveState.Idle:
                    break;
                case MoveState.Sliding:
                    if (rigidbody.velocity.magnitude < Mathf.Epsilon)
                    {
                        if (onMoveComplete != null)
                        {
                            currentState = MoveState.Idle;
                            onMoveComplete();
                            onMoveComplete = null;
                        }
                    }
                    break;
                case MoveState.Busy:
                    break;
                case MoveState.Move:
                    var sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    float moveSpeed = 4f;
                    if (sqrMag > lastSqrMag)
                    {
                        if (onMoveComplete != null)
                        {
                            currentState = MoveState.Idle;
                            onMoveComplete();
                            onMoveComplete = null;
                        }
                    }
                    else
                    {
                        rigidbody.velocity = (slideTargetPosition - transform.position).normalized * moveSpeed;
                    }
                    lastSqrMag = sqrMag;
                    break;
                case MoveState.Knockback:
                    if (rigidbody.velocity.magnitude < 0 + float.Epsilon)
                        currentState = MoveState.Idle;
                    break;
                case MoveState.Slerp:
                    sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;
                    moveSpeed = 4f;
                    rigidbody.transform.position = (Vector3.Slerp(rigidbody.transform.position, slideTargetPosition, 0.01f));

                    if (sqrMag > lastSqrMag)
                    {
                        rigidbody.velocity = Vector3.zero;
                        currentState = MoveState.Idle;
                        onMoveComplete();
                    }

                    lastSqrMag = sqrMag;
                    break;
            }
        }


        public void Act(Action onActorActionFinished)
        {
            //if flying in midair do a check to 




            var bm = BattleManager.instance;
            var im = InputManager.instance;
            var um = UIManager.GetInstance();

            UIManager.GetInstance().ChangeStatus(Actor.Name + " TO ACT");
            if (isControllable())
            {
                var AvaliableSkills = Actor.actions;
                Time.timeScale = 0f;
                um.DrawActionsAndWaitForSelectionOrNull(AvaliableSkills, selectedSkill =>
                {
                    if (selectedSkill == null)
                    {
                        Time.timeScale = 1f;

                        StartCoroutine(WaitForOneFrame(onActorActionFinished));
                    }
                    else
                    {
                        if (selectedSkill.Tags.Contains(TAG.USESLIDER))
                            selectedSkill.SliderValue = InputManager.instance.SliderValue;
                        if (selectedSkill.Tags.Contains(TAG.USEKNOB))
                            selectedSkill.StickValue = InputManager.instance.StickValue;

                        Time.timeScale = 1f;
                        UseAction(selectedSkill, () => { onActorActionFinished(); Debug.Log("Player finished turn, going back to battlemanager"); });
                    }
                });
            }
            else
            {
                //Enemy AI here basically
                var chosenSkill = ChooseValidSkillInRangeOrReturnNull();
                if (chosenSkill == null)
                {
                    var movedirection = (bm.PlayerActors[0].transform.position - transform.position).normalized;
                    MoveSkill.instance.StickValue = new Vector2(movedirection.x, movedirection.z);
                    UseAction(MoveSkill.instance, onActorActionFinished);
                }
                else
                {
                    chosenSkill.SliderValue = 1f;
                    chosenSkill.StickValue = Vector2.zero;
                    UseAction(chosenSkill, () => { onActorActionFinished(); Debug.Log("Enemy finished turn, going back to bm"); });
                }

            }
        }
        public void UseAction(BaseAction action, Action onSkillComplete)
        {
            var bm = BattleManager.instance;

            //Hack
            bm.PlayerActors[0].transform.rotation = Quaternion.LookRotation(bm.EnemyActors[0].transform.position - bm.PlayerActors[0].transform.position, Vector3.up);
            bm.EnemyActors[0].transform.rotation = Quaternion.LookRotation(bm.PlayerActors[0].transform.position - bm.EnemyActors[0].transform.position, Vector3.up);

            //UI draw action above head
            UIManager.GetInstance().DrawActionAboveHead(this, action);


            UIManager.GetInstance().SetTextThenFade($"{Actor.Name} uses {action.Name}!", 0.5f);

            action.Perform(this, () => { UIManager.GetInstance().KillActionAboveHead(this); onSkillComplete(); });
        }


        public List<BaseAction> GetValidContinueComboSkills()
        {
            return Actor.actions.Where(skill => skill.IsValid(this, out _) && !skill.Tags.Contains(TAG.STARTER)).ToList();
        }
        public List<BaseAction> GetValidStartComboSkills()
        {
            return Actor.actions.Where(skill => skill.IsValid(this, out _)).ToList();
        }
        public List<BaseReaction> GetValidReactionsForSkill()
        {
            var reactions = Actor.reactions.Where(reaction => reaction.IsValid(this, out _)).ToList();
            return reactions;
        }
        public BaseAction ChooseValidSkillInRangeOrReturnNull()
        {
            List<BaseAction> validskills = new List<BaseAction>();

            validskills = Actor.actions.Where(skills => skills.IsValidAndInRange(this)).ToList();

            if (validskills.Count == 0) return null;

            var random = new System.Random();
            var index = random.Next(validskills.Count);

            return validskills[index];
        }


        public void ApplyPosture(float postureDamage)
        {
            float postMitigationDamage = postureDamage;
            if (ApplyPostureModifiers != null)
            {
                postMitigationDamage = ApplyPostureModifiers(postureDamage);
            }

            Actor.DealPostureDamage(postMitigationDamage);
            ShowPosturePopup(postMitigationDamage);

            if (Actor.currentPosture <= Actor.maxPosture / 2 && !activeStates.Any(state => state is Stagger))
            {
                var stagger = new Stagger(this);
                activeStates.Add(stagger);
            }
        }
        public void ApplyDamage(float preMitigationDamage)
        {
            float postMitgationDamage = preMitigationDamage;
            if (ApplyDamageModifiers != null)
            {
                postMitgationDamage = ApplyDamageModifiers(preMitigationDamage);
            }
            Actor.DealDamage(postMitgationDamage);

            ShowDamagePopup(postMitgationDamage);

            PlayAudio("Blow1");
        }
        public void ApplyKnockback(Vector3 direction, float force)
        {
            var animator = this.GetComponent<Animator>();
            if (force > 0)
            {
                float postMitigationForce = force;
                Debug.Log("preMitigationForce is " + postMitigationForce);
                Debug.Log("preMitigationDirection is " + direction);

                if (ApplyKnockbackModifiers != null)
                {
                    postMitigationForce = ApplyKnockbackModifiers(force, direction);
                }

                Debug.Log("postMitigationForce is " + postMitigationForce);
                Debug.Log("postMitigationDirection is " + direction);

                rigidbody.AddForce(direction * 100 * postMitigationForce);


                //this.MoveToPosition(transform.position + direction.normalized * force, MoveState.Sliding, () => { animator.Play("Idle"); onKnockbackFinished(); });
                //Blood particles
                //GetComponentInChildren<ParticleSystem>().Play();
            }
        }


        public void Move(Vector3 direction, Action onMoveComplete)
        {
            PlayAudio("Move2");

            this.slideTargetPosition = transform.position + (direction * Actor.AGI);
            this.onMoveComplete = onMoveComplete;
            this.currentState = MoveState.Move;
            lastSqrMag = Mathf.Infinity;
        }
        public void MoveToPosition(Vector3 TargetPosition, MoveState state, Action onMoveComplete)
        {
            if (state == MoveState.Sliding)
                rigidbody.velocity = (TargetPosition - transform.position);

            lastSqrMag = Mathf.Infinity;
            this.slideTargetPosition = TargetPosition;
            this.onMoveComplete = onMoveComplete;
            this.currentState = state;
        }
        public void MoveRelativeToCamera(Vector2 direction, Action onMoveComplete)
        {
            var camera = Camera.main;
            var forward = camera.transform.forward; forward.y = 0;
            var right = camera.transform.right; right.y = 0;
            forward.Normalize(); right.Normalize();

            var desiredMoveDirection = forward * direction.y + right * direction.x;
            Move(desiredMoveDirection, onMoveComplete);
        }


        public void PlayAnimation(string AnimationName, Action onAnimationHit, Action onReactionCheck, Action onAnimationEnd)
        {
            animator.Play(AnimationName, -1, 0);
            this.onReactionCheck = onReactionCheck;
            this.onAnimationHitComplete = onAnimationHit;
            this.onAnimationEndComplete = onAnimationEnd;
        }
        public void PlayAnimation(string AnimationName, Action onAnimationHit, Action onAnimationEnd)
        {
            animator.Play(AnimationName, -1, 0);
            this.onAnimationHitComplete = onAnimationHit;
            this.onAnimationEndComplete = onAnimationEnd;
        }
        public void PlayAnimation(string AnimationName)
        {
            animator.Play(AnimationName, -1, 0);
        }

        public void KillAnimationEndEvent()
        {
            this.onAnimationEndComplete = null;
        }


        public void ProcNextTurnEffects()
        {
            collisionOccured = false;
            foreach (var skill in Actor.actions)
            {
                skill.UpdateCooldown();
            }
            foreach (var reaction in Actor.reactions)
            {
                reaction.UpdateCooldown();
            }

            if (activeStates.OfType<Stagger>().Any())
            {
                Debug.Log("Remove Stagger - Set Posture to max");
                activeStates.OfType<Stagger>().First().Remove();
                Actor.currentPosture = Actor.maxPosture;
                PlayAnimation("Idle");
            }
        }
        public bool isControllable()
        {
            return Actor.Controllable;
        }



        //Do not touch
        public void AnimationHit()
        {
            if(onAnimationHitComplete != null)
            {
                onAnimationHitComplete();
                onAnimationHitComplete = null;
            }
        }
        public void AnimationEnd()
        {
            if (onAnimationEndComplete != null)
            {
                onAnimationEndComplete();
                onAnimationEndComplete = null;
            }
        }
        public void ReactionCheck()
        {
            onReactionCheck();
            onReactionCheck = null;
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
        public System.Collections.IEnumerator WaitForOneFrame(Action action)
        {
            // This will wait for one frame
            yield return new WaitForSeconds(0.15f);

            // Code here will be executed on the frame after the wait
            action.Invoke();
        }
        private void OnCollisionEnter(Collision collision)
        {
            // Check if collided object has the "Level" tag
            if (collisionOccured) return;

            if (collision.gameObject.CompareTag("Level"))
            {
                Debug.Log($"Velocity on collision is {rigidbody.velocity.magnitude}");

                UIManager.GetInstance().AddToStoneSlab($"{Actor.Name} hits a wall!");
                ApplyPosture(5f);
                collisionOccured = true;

                // Check if velocity magnitude is greater than the threshold
                if (rigidbody.velocity.magnitude > 0.25f)
                {
                    // Calculate mirrored velocity (mirror along current velocity)
                    Vector3 mirroredVelocity = Vector3.Reflect(rigidbody.velocity, collision.contacts[0].normal);

                    // Replace current velocity with the mirrored velocity
                    rigidbody.velocity = mirroredVelocity * 2f;
                }
            }
        }
        private void TriggerHitstop(float duration)
        {
            Time.timeScale = 0.0f; // Pause the game
            StartCoroutine(ResumeTimeScale(duration));
        }
        private IEnumerator ResumeTimeScale(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f; // Restore the original time scale
        }
        private void ShowDamagePopup(float damageAmount)
        {
            // Instantiate the damage popup prefab
            GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);

            popup.GetComponentInChildren<DamagePopup>().Initialize(damageAmount);

        }
        private void ShowPosturePopup(float damageAmount)
        {
            // Instantiate the damage popup prefab
            GameObject popup = Instantiate(posturePopupPrefab, transform.position, Quaternion.identity);

            // Set the damage amount text
            popup.GetComponentInChildren<DamagePopup>().Initialize(damageAmount);

        }
        private int RandomFromList(int count)
        {
            var random = new System.Random();
            var index = random.Next(count);
            return index;
        }

        private bool IsGrounded()
        {
            // Perform a raycast from the object's position downward
            Ray ray = new Ray(transform.position, Vector3.down);

            // Check if the ray hits something within the specified distance
            if (Physics.Raycast(ray, 0.01f))
            {
                return true; // The object is grounded
            }

            return false; // The object is not grounded
        }
    }

    public enum MoveState
    {
        Idle,
        Midair,
        Knockback,
        Sliding,
        Move,
        Busy,
        Slerp,
    }

}
