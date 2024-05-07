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
    public class Actor : MonoBehaviour
    {
        [SerializeField] public ActorData ActorData;

        // Movement and Animation
        private MoveState currentState;
        private Vector3 slideTargetPosition;
        private Action onMoveComplete;
        private Action onAnimationHitComplete;
        private Action onAnimationEndComplete;
        private Action onReactionCheck;
        private Animator animator;
        public new Rigidbody rigidbody;
        private float lastSqrMag;
        // Audio
        private AudioSource audioSource;
        // Collision
        internal bool collisionOccured;
        // Prefabs and Objects
        private static GameObject damagePopupPrefab;
        private static GameObject posturePopupPrefab;
        public LayerMask layer;
        // States and Actions
        public List<BaseStatus> activeStates;
        public event DamageModifier DamageDealt;
        public delegate float DamageModifier(float damage);
        public delegate float PostureModifier(float damage);
        public delegate float KnockbackModifier(float force, Vector3 direction);
        
        public event PostureModifier PostureDamageDealt;
        public event KnockbackModifier KnockbackDealt;
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
            if(damagePopupPrefab == null)
                damagePopupPrefab = Resources.Load<GameObject>("HpPopup");
            if (posturePopupPrefab == null)
                posturePopupPrefab = Resources.Load<GameObject>("PosturePopup");
            //var cc = GetComponentInChildren<ColorController>();
            //cc.mainColor = baseActor.mainColor;
            //cc.secondaryColor = baseActor.secondaryColor;
        }
        private void FixedUpdate()
        {
            if (IsGrounded()) activeStates.RemoveAll(state => state is Midair);
            switch (currentState)
            {
                case MoveState.Move:
                    float moveSpeed = 2f;
                    float minDistance = 0.1f; // Define a minimum distance threshold
                    float sqrMag = (slideTargetPosition - transform.position).sqrMagnitude;

                    if (sqrMag <= minDistance * minDistance || sqrMag > lastSqrMag)
                    {
                        // If already close to the target position or moving away, transition to Idle state
                        currentState = MoveState.Idle;
                        if (onMoveComplete != null)
                        {
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
            }
        }


        public void Act(Action onActorActionFinished) //Chose and perform an action
        {
            //if flying in midair do a check to 
            var bm = BattleManager.instance;
            var im = InputManager.instance;
            var um = UIManager.GetInstance();

            Debug.Log(ActorData.Name + " TO ACT");
            if (isControllable())
            {
                var AvaliableSkills = ActorData.actions;
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
                        //if (selectedSkill.Tags.Contains(TAG.USESLIDER))
                        //    selectedSkill.SliderValue = InputManager.instance.SliderValue;
                        //if (selectedSkill.Tags.Contains(TAG.USEKNOB))
                        //    selectedSkill.StickValue = InputManager.instance.StickValue;

                        Time.timeScale = 1f;
                        UseAction(selectedSkill, () => { onActorActionFinished(); Debug.Log("Player finished turn, going back to battlemanager"); });
                    }
                });
            }
            else
            {
                onActorActionFinished();
                return;
                //Enemy AI here basically
                var chosenSkill = ChooseValidSkillInRangeOrReturnNull();
                if (chosenSkill == null)
                {
                    Debug.Log("Enemy moving closer");
                    var movedirection = (bm.PlayerActors[0].transform.position - transform.position).normalized;
                    MoveAction.instance.StickValue = new Vector2(movedirection.x, movedirection.z);
                    UseAction(MoveAction.instance, onActorActionFinished);
                }
                else
                {
                    chosenSkill.SliderValue = 1f;
                    chosenSkill.StickValue = Vector2.zero;
                    UseAction(chosenSkill, () => { onActorActionFinished(); Debug.Log("Enemy finished turn, going back to bm"); });
                }

            }
        }
        public void UseAction(BaseAction action, Action onSkillComplete) //Perform an action
        {
            var bm = BattleManager.instance;

            //Hack
            bm.PlayerActors[0].transform.rotation = Quaternion.LookRotation(bm.EnemyActors[0].transform.position - bm.PlayerActors[0].transform.position, Vector3.up);
            bm.EnemyActors[0].transform.rotation = Quaternion.LookRotation(bm.PlayerActors[0].transform.position - bm.EnemyActors[0].transform.position, Vector3.up);

            //UI draw action above head
            UIManager.GetInstance().DrawActionAboveHead(this, action);


            UIManager.GetInstance().SetTextThenFade($"{ActorData.Name} uses {action.Name}!", 0.5f);

            action.Perform(this, () => { UIManager.GetInstance().KillActionAboveHead(this); onSkillComplete(); });
        }


        public List<BaseAction> GetValidContinueComboSkills()
        {
            return ActorData.actions.Where(skill => skill.IsValid(this, out _) && !skill.Tags.Contains(TAG.STARTER)).ToList();
        }
        public List<BaseAction> GetValidStartComboSkills()
        {
            return ActorData.actions.Where(skill => skill.IsValid(this, out _)).ToList();
        }
        public List<BaseReaction> GetValidReactionsForSkill()
        {
            var reactions = ActorData.reactions.Where(reaction => reaction.IsValid(this, out _)).ToList();
            return reactions;
        }
        public BaseAction ChooseValidSkillInRangeOrReturnNull()
        {
            List<BaseAction> validskills = new List<BaseAction>();

            validskills = ActorData.actions.Where(skills => skills.IsValidAndInRange(this)).ToList();

            if (validskills.Count == 0) return null;

            var random = new System.Random();
            var index = random.Next(validskills.Count);

            return validskills[index];
        }


        public void ApplyPosture(float originalPostureDamage)
        {
            float postMitigationDamage = originalPostureDamage;
            if (PostureDamageDealt != null)
            {
                postMitigationDamage = PostureDamageDealt(originalPostureDamage);
            }

            ActorData.DealPostureDamage(postMitigationDamage);
            ShowPosturePopup(postMitigationDamage);

            if (ActorData.currentPosture <= ActorData.maxPosture / 2 && !activeStates.Any(state => state is Stagger))
            {
                var stagger = new Stagger(this);
                activeStates.Add(stagger);
            }
        }
        public void ApplyDamage(float originalDamage)
        {
            float postMitgationDamage = originalDamage;
            if (DamageDealt != null)
            {
                postMitgationDamage = DamageDealt(originalDamage);
            }
            ActorData.DealDamage(postMitgationDamage);
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

                if (KnockbackDealt != null)
                {
                    postMitigationForce = KnockbackDealt(force, direction);
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

            this.slideTargetPosition = transform.position + (direction * ActorData.AGI);
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
            switch (ActorData.currentStamina / ActorData.maxStamina)
            {
                case float n when (n >= 0 && n < 0.33f):
                    ActorData.DealStaminaDamage(-5f);
                    break;

                case float n when (n >= 0.33 && n < 0.66):
                    ActorData.DealStaminaDamage(-10f);
                    break;

                case float n when (n >= 0.66 && n <= 1):
                    ActorData.DealStaminaDamage(-7.5f);
                    break;
            }


            foreach (var skill in ActorData.actions)
            {
                skill.UpdateCooldown();
            }
            foreach (var reaction in ActorData.reactions)
            {
                reaction.UpdateCooldown();
            }

            if (activeStates.OfType<Stagger>().Any())
            {
                Debug.Log("Remove Stagger - Set Posture to max");
                activeStates.OfType<Stagger>().First().Remove();
                ActorData.currentPosture = ActorData.maxPosture;
                PlayAnimation("Idle");
            }
        }
        public bool isControllable()
        {
            return ActorData.Controllable;
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

                UIManager.GetInstance().AddToStoneSlab($"{ActorData.Name} hits a wall!");
                ApplyPosture(5f);
                collisionOccured = true;

                // Check if velocity magnitude is greater than the threshold
                if (collision.relativeVelocity.magnitude > 0.1f)
                {
                    // Calculate mirrored velocity (mirror along current velocity)
                    Vector3 mirroredVelocity = Vector3.Reflect(rigidbody.velocity, collision.GetContact(0).normal);
                    var r = collision.relativeVelocity - 2 * Vector3.Dot(rigidbody.velocity,collision.GetContact(0).normal) * collision.contacts[0].normal;


                    // Replace current velocity with the mirrored velocity
                    rigidbody.velocity = r;
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
