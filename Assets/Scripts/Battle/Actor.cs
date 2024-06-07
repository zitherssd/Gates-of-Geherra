using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Components.Audio;
using Assets.Scripts.Battle.Components.Effects;
using Assets.Scripts.Battle.Components.State;
using Assets.Scripts.Battle.Components.Status;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Battle
{
    public class Actor : MonoBehaviour
    {
        //Data
        [SerializeField] public ActorData ActorData;

        //Components
        public StateMachine state;
        public StatusManager statusManager; 
        public new AudioManager audio;
        public EffectManager effects;

        //Events
        public delegate float DamageValue(float damage);
        public delegate float PostureValue(float damage);
        public delegate float KnockbackValue(float force, Vector3 direction);

        public event DamageValue DamageRecieved;
        public event PostureValue PostureRecieved;
        public event KnockbackValue KnockbackRecieved;

        public event Action<float> DamageApplied;
        public event Action<float> PostureApplied;
        public event Action<float, Vector3> KnockbackApplied;

        // UI
        public OriginPointHandler originPointInUI;

        // Private members
        private bool hitLastRound;
        private Action onAnimationHitComplete;
        private Action onAnimationEndComplete;
        private Action onReactionCheck;
        private Animator animator;
        public new Rigidbody rigidbody { get; set; }



        private void Awake()
        {
            rigidbody = gameObject.GetComponent<Rigidbody>();
            animator = gameObject.GetComponent<Animator>();



            state = new StateMachine(this);
            audio = new AudioManager(this);
            statusManager = new StatusManager(this);
            effects = new EffectManager(this);
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
                Time.timeScale = 0f; UIManager.GetInstance().ShowUI();
                um.DrawActionsAndWaitForSelectionOrNull(AvaliableSkills, selectedSkill =>
                {
                    if (selectedSkill == null)
                    {
                        Time.timeScale = 1f; UIManager.GetInstance().HideUI();
                        Debug.Log("Player skipped");
                        StartCoroutine(WaitForOneFrame(onActorActionFinished));
                    }
                    else
                    {
                        //if (selectedSkill.Tags.Contains(TAG.USESLIDER))
                        //    selectedSkill.SliderValue = InputManager.instance.SliderValue;
                        //if (selectedSkill.Tags.Contains(TAG.USEKNOB))
                        //    selectedSkill.StickValue = InputManager.instance.StickValue;

                        Time.timeScale = 1f; UIManager.GetInstance().HideUI();

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
                    Debug.Log("Enemy moving closer");
                    var movedirection = (bm.PlayerActors[0].transform.position - transform.position).normalized;
                    var moveSkill = ActorData.actions.OfType<MoveAction>().First();
                    if (moveSkill)
                    {
                        moveSkill.StickValue = new Vector2(movedirection.x, movedirection.z);
                        UseAction(moveSkill, onActorActionFinished);
                        return;
                    }
                    else
                    {
                        onActorActionFinished.Invoke();
                    }
                    //UseAction(MoveAction.instance, () => { Debug.Log("Enemy finished moving, back to bm"); onActorActionFinished(); });
                }
                else
                {
                    chosenSkill.SliderValue = 1f;
                    var direction = (bm.PlayerActors[0].transform.position - transform.position).normalized;
                    var directionleft = Vector3.Cross(Vector3.up, direction).normalized;
                    directionleft = directionleft * UnityEngine.Random.Range(-0.3f, 0.3f);
                    var dirleftvector = new Vector3(directionleft.x, directionleft.z);
                    chosenSkill.StickValue = new Vector3(direction.x, direction.z) + dirleftvector;
                    UseAction(chosenSkill, () => { Debug.Log("Enemy finished turn, going back to bm"); onActorActionFinished(); });
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
        public void Update()
        {
            state.Update();
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
            if (PostureRecieved != null)
            {
                postMitigationDamage = PostureRecieved(originalPostureDamage);
            }

            PostureApplied.Invoke(postMitigationDamage);
            ActorData.DealPostureDamage(postMitigationDamage);

            if(state.CurrentState != state.staggerState)
            {
                if (ActorData.currentPosture <= ActorData.maxPosture / 2)
                {
                    state.TransitionTo(state.staggerState);
                }
            }
            else
            {
                //al;ready stagger state

            }
            
        }
        public void ApplyDamage(float originalDamage)
        {
            float postMitgationDamage = originalDamage;
            if (DamageRecieved != null)
            {
                postMitgationDamage = DamageRecieved(originalDamage);
            }

            DamageApplied?.Invoke(postMitgationDamage);
            ActorData.DealDamage(postMitgationDamage);

            hitLastRound = true;
        }
        public void ApplyKnockback(Vector3 direction, float force)
        {
            var animator = this.GetComponent<Animator>();
            if (force > 0)
            {
                float postMitigationForce = force;
                Debug.Log("preMitigationForce is " + postMitigationForce);
                Debug.Log("preMitigationDirection is " + direction);

                if (KnockbackRecieved != null)
                {
                    postMitigationForce = KnockbackRecieved(force, direction);
                }

                Debug.Log("postMitigationForce is " + postMitigationForce);
                Debug.Log("postMitigationDirection is " + direction);

                rigidbody.AddForce(direction * 100 * postMitigationForce);
                KnockbackApplied?.Invoke(100 * postMitigationForce, direction);


                //this.MoveToPosition(transform.position + direction.normalized * force, MoveState.Sliding, () => { animator.Play("Idle"); onKnockbackFinished(); });
                //Blood particles
                //GetComponentInChildren<ParticleSystem>().Play();
            }
        }


        public void Move(Vector3 direction, Action onMoveComplete)
        {
            var TargetPosition = transform.position + (direction * ActorData.AGI);

            //state.TransitionTo(new MoveState(this, TargetPosition, onMoveComplete));
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
            if (ActorData.currentPosture <= ActorData.maxPosture / 2)
            {
                ActorData.currentPosture = ActorData.maxPosture;
                hitLastRound = false;
            }
            else
            {
                if (hitLastRound)
                    hitLastRound = false;
                else
                    ApplyPosture(-5f);
            }
            switch (ActorData.currentStamina / ActorData.maxStamina)
            {
                case float n when (n >= 0 && n < 0.33f):
                    ActorData.DealStaminaDamage(-5f);
                    break;

                case float n when (n >= 0.33 && n < 0.66):
                    ActorData.DealStaminaDamage(-10f);
                    break;

                case float n when (n >= 0.66 && n <= 1):
                    ActorData.DealStaminaDamage(-3f);
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

            if (state.CurrentState == state.staggerState)
            {
                Debug.Log("Remove Stagger - Set Posture to max");
                state.TransitionTo(state.idleState);
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
            if (onAnimationHitComplete != null)
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

        public System.Collections.IEnumerator WaitForOneFrame(Action action)
        {
            // This will wait for one frame
            yield return new WaitForSeconds(0.15f);

            // Code here will be executed on the frame after the wait
            action.Invoke();
        }
        public System.Collections.IEnumerator WaitForTime(Action action, float seconds)
        {
            // This will wait for one frame
            yield return new WaitForSeconds(seconds);

            // Code here will be executed on the frame after the wait
            action.Invoke();
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Level"))
            {
                state.OnCollisionEnter(collision);
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
        private bool IsGrounded()
        {
            // Perform a raycast from the object's position downward
            Ray ray = new Ray(transform.position + Vector3.up * 0.01f, Vector3.down);

            // Check if the ray hits something within the specified distance
            if (Physics.Raycast(ray, 0.02f))
            {
                return true; // The object is grounded
                Debug.Log(ActorData.Name + " GROUNDED");
            }
            Debug.Log(ActorData.Name + " NOT GROUNDED");
            return false; // The object is not grounded
        }

        public bool grounded { get { return IsGrounded(); } private set { } }
    }
}
