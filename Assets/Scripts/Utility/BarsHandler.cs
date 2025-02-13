using Assets;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Components.State;
using Assets.Scripts.Battle.Components.Status;
using Assets.Scripts.Pattern;
using System.Collections.Generic;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class BarsHandler : MonoBehaviour
{
    private Actor actor;
    private ActorData actorData;

    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider hpBarEase;
    [SerializeField] private TextMeshProUGUI hpBarText;
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
    [SerializeField] private TextMeshProUGUI statesText;
    [SerializeField] private CanvasGroup canvasGroup; // Required for fading
    [SerializeField] private Slider ccBar;
    [SerializeField] private TextMeshProUGUI ccBarText;
    [SerializeField] private TextMeshProUGUI ccBarDurationText;
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
        if(ccBar.enabled)
        {

        trackingCCDuration -= Time.deltaTime;
        ccBar.value = trackingCCDuration / lastSetDuration;
        ccBarDurationText.text = trackingCCDuration.ToString("F2");
        if (trackingCCDuration < 0f)
            HideCC();
        }

    }

    public void Start()
    {

        actor = GetComponent<Actor>();
    }

    public void Initialize(Actor actor)
    {
        this.actor = actor;
        this.actorData = actor.ActorData;
        actor.state.stateChanged += OnStateChanged;
        lastStamina = actor.ActorData.currentStamina;
        staminaBar.value = lastStamina;
        staminaBarEase.value = lastStamina;
        this.actor.state.staggerState.OnStaggerStateEntered += SetStun;

        foreach (Transform child in hpBarsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var hpBar in actorData.hpBars)
        {
            var bar = Instantiate(hpBarPrefab, hpBarsContainer);
            bar.GetComponent<HpBarHandler>().bar = hpBar;
        }

        UpdateVisibility();
    }

    private void OnStateChanged(IState state)
    {
        if (statesText != null)
        {
            var name = state.GetType().Name;
            statesText.text = "Lv.1 " + $"{actorData.name}";
        }
    }

    void DamageStamina()
    {
        LeanTween.cancel(staminaBar.fillRect);
        LeanTween.cancel(staminaBarEase.fillRect);

        LeanTween.alpha(staminaBarEase.fillRect, 1f, 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);
        LeanTween.alpha(staminaBar.fillRect, 1f, 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);

        staminaBar.value = actorData.currentStamina / actorData.maxStamina;

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
        float newvalue = actorData.currentStamina / actorData.maxStamina;
        staminaBar.value = newvalue;
        staminaBarEase.value = newvalue;
    }

    void Update()
    {
        if (actorData == null && actor)
            actorData = actor.ActorData;

        if (lastStamina < actorData.currentStamina)
        {
            HealStamina();
        }
        else if (lastStamina > actorData.currentStamina)
        {
            DamageStamina();
        }
        lastStamina = actor.ActorData.currentStamina;

        hpBar.value = actorData.GetCurrentHP() / actorData.maxHp;
        hpBarEase.value = Mathf.Lerp(hpBarEase.value, hpBar.value, 0.01f);

        postureBar.value = actorData.currentPosture / actorData.maxPosture;
        postureBarEase.value = Mathf.Lerp(postureBarEase.value, postureBar.value, 0.01f);

        buildupBar.value = actorData.currentBuildup / actorData.maxBuildup;
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
}
