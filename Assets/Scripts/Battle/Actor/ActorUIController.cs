using System.Collections.Generic;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor.States;
using Assets.Scripts.Battle.Components.Status;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Pattern;
using Assets.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Battle.Actor
{
    public class ActorUIController : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
    
    
        //Reference to parent actor
        private Actor actor;

        //Static references to bars on the prefab object
        [SerializeField] private Slider hpBar;
        [SerializeField] private Slider hpBarEase;
        [SerializeField] private Slider postureBar;
        [SerializeField] private Slider postureBarEase;
        [SerializeField] private TextMeshProUGUI postureBarText;
        [SerializeField] private Slider buildupBar;
        [SerializeField] private Slider buildupBarEase;
        [SerializeField] private TextMeshProUGUI buildupBarText;
        [SerializeField] private Slider staminaBar;
        [SerializeField] private Slider staminaBarEase;
        [SerializeField] private RectTransform hpBarsContainer;
        [SerializeField] private GameObject hpBarPrefab;
        [SerializeField] public TextMeshProUGUI statesText;
        [SerializeField] private Slider ccBar;
        [SerializeField] private TextMeshProUGUI ccBarText;
        [SerializeField] private TextMeshProUGUI ccBarDurationText;
        [SerializeField] private Image windupIndicator;
        [SerializeField] private CanvasGroup windupCanvasGroup;
        [SerializeField] private Gradient windupColorOverTime;
        private float lastSetDuration;
        private float trackingCCDuration;
        private float lastStamina;

        // Public field to be controlled from outside
        private bool _show = true;
        public bool show
        {
            get => _show;
            set
            {
                if (_show != value)
                {
                    _show = value;
                    UpdateVisibility();
                }
            }
        }
        public void SetStun(float duration)
        { SetCC(duration, "Stun"); }
        public void SetCC(float duration, string text)
        {
            HideCC();
            ccBarText.gameObject.SetActive(true);
            ccBar.gameObject.SetActive(true);
            ccBarDurationText.gameObject.SetActive(true);
            lastSetDuration = duration;
            trackingCCDuration = duration;
            ccBarText.text = text;
        }

        public void HideCC()
        {
            ccBar.gameObject.SetActive(false);
            ccBarText.gameObject.SetActive(false);
            ccBarDurationText.gameObject.SetActive(false);
            ccBarText.text = string.Empty;
            lastSetDuration = 0f;
            trackingCCDuration = 0f;
        }

        private void HandleCCBar()
        {
            if(ccBar.IsActive())
            {

                trackingCCDuration -= Time.deltaTime;
                ccBar.value = trackingCCDuration / lastSetDuration;
                ccBarDurationText.text = trackingCCDuration.ToString("F2");
                if (trackingCCDuration < 0f)
                    HideCC();
            }

        }

        public void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            actor = GetComponentInParent<Actor>();
        }

        public void SetProgress(float t)
    {
            if(t != 0)
            LeanTween.alphaCanvas(windupCanvasGroup, 1f, 0.1f).setIgnoreTimeScale(true);
            else if(t<=0f)
            {
                LeanTween.alphaCanvas(windupCanvasGroup, 0f, 0.5f).setIgnoreTimeScale(true);
                windupIndicator.color = windupColorOverTime.Evaluate(1);
            }

            windupIndicator.fillAmount = Mathf.Clamp01(t);

            windupIndicator.color = windupColorOverTime.Evaluate(t);
    }

        public void ResetIndicator()
    {
            LeanTween.alphaCanvas(windupCanvasGroup, 0f, 0.1f).setIgnoreTimeScale(true);

            windupIndicator.fillAmount = 0f;
            windupIndicator.color = windupColorOverTime.Evaluate(0f);
    }

        public void Start()
        {
            statesText.text = actor.Runtime.Name;
            actor.state.StateChanged += OnStateChanged;
            actor.Runtime.OnDeath += HideAllBars;
            BattleManager.instance.battleStateMachine.activeState.FinalHitDealth += HideAllBars;
            BattleManager.instance.battleStateMachine.startState.OnNewBattle += ShowAllBars;
            lastStamina = actor.Runtime.currentStamina;
            staminaBar.value = lastStamina;
            staminaBarEase.value = lastStamina;
            var staggerState = actor.state.GetState<StaggerState>();
            staggerState.OnStaggerStateEntered += SetStun;
            staggerState.OnStaggerStateExit += HideCC;
            SetupWindupIndicator(actor.state.GetState<ActingState>());
            foreach (Transform child in hpBarsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var hpBar in actor.Runtime.hpBars)
            {
                var bar = Instantiate(hpBarPrefab, hpBarsContainer);
                bar.GetComponent<HpBarHandler>().bar = hpBar;
            }

            UpdateVisibility();
        }

        private void SetupWindupIndicator(ActingState actingState)
        {
            actingState.onWindupProgress += SetProgress;
            actingState.onEnterRecovery += _ => ResetIndicator();
            actingState.onHit += ResetIndicator;
            actingState.onEnd += ResetIndicator;
        }

        private void OnStateChanged(IState state)
        {
            if (statesText != null)
            {
                var name = actor.Runtime.Name.Replace("(Clone)", "").Trim();
                statesText.text = name;
            }
        }

        private void OnDestroy()
        {
            LeanTween.cancel(gameObject);

            BattleManager.instance.battleStateMachine.activeState.FinalHitDealth -= HideAllBars;
            BattleManager.instance.battleStateMachine.startState.OnNewBattle -= ShowAllBars;
        }

        void DamageStamina()
        {
            LeanTween.cancel(staminaBar.fillRect);
            LeanTween.cancel(staminaBarEase.fillRect);

            LeanTween.alpha(staminaBarEase.fillRect, 1f, 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);
            LeanTween.alpha(staminaBar.fillRect, 1f, 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);

            staminaBar.value = actor.Runtime.currentStamina / actor.Runtime.maxStamina;

            LeanTween.value(staminaBarEase.value, staminaBar.value, 2f)
                .setEaseOutCubic()
                .setIgnoreTimeScale(true)
                .setOnUpdate((float val) =>
                {
                    staminaBarEase.value = val;
                });
        }

        void HealStamina()  
        {
            float newvalue = actor.Runtime.currentStamina / actor.Runtime.maxStamina;
            staminaBar.value = newvalue;
            staminaBarEase.value = newvalue;
        }

        void Update()
        {
            if (lastStamina < actor.Runtime.currentStamina)
            {
                HealStamina();
            }
            else if (lastStamina > actor.Runtime.currentStamina)
            {
                DamageStamina();
            }
            lastStamina = actor.Runtime.currentStamina;

            //hpBar.value = actor.Runtime.GetCurrentHP() / actor.Runtime.maxHp;
            //hpBarEase.value = Mathf.Lerp(hpBarEase.value, hpBar.value, 0.01f);

            postureBar.value = actor.Runtime.currentPosture / actor.Runtime.maxPosture;
            postureBarEase.value = Mathf.Lerp(postureBarEase.value, postureBar.value, 0.01f);

            buildupBar.value = actor.Runtime.currentBuildup / actor.Runtime.maxBuildup;
            buildupBarEase.value = Mathf.Lerp(buildupBarEase.value, buildupBar.value, 0.01f);
            HandleCCBar();
        }

        public string WriteStatusTypes(List<BaseStatus> statusLists)
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            foreach (BaseStatus status in statusLists)
            {
                result.Append(status.GetType().Name).Append(", ");
            }

            if (result.Length > 0)
            {
                result.Length -= 2;
            }
            return result.ToString();
        }

        private void UpdateVisibility()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            if (_show)
            {
                LeanTween.alphaCanvas(canvasGroup, 1f, 0.5f).setEaseOutCubic();
            }
            else
            {
                LeanTween.alphaCanvas(canvasGroup, 0f, 0.5f).setEaseOutCubic();
            }
        }

        private void HideAllBars()
        {
            canvasGroup.LeanAlpha(0, 1f).setEaseOutBack().setIgnoreTimeScale(true);
        }
        private void ShowAllBars()
        {
            canvasGroup.LeanAlpha(1, 1f).setEaseOutBack().setIgnoreTimeScale(true);
        }
    }
}
