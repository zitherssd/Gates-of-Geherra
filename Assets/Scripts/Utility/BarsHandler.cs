using Assets;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Components.State;
using Assets.Scripts.Battle.Components.Status;
using Assets.Scripts.Pattern;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] TextMeshProUGUI statesText;

    private LTDescr x;
    private LTDescr y;

    private float lastStamina;
    // Start is called before the first frame update
    void Start()
    {
        actor = gameObject.GetComponentInParent<Actor>();
        actorData = actor.ActorData;
        actor.state.stateChanged += OnStateChanged;
        lastStamina = actor.ActorData.currentStamina;
        staminaBar.value = lastStamina;
        staminaBarEase.value = lastStamina;
        foreach (var hpBar in actorData.hpBars)
        {
            var bar = Instantiate(hpBarPrefab, hpBarsContainer);
            bar.GetComponent<HpBarHandler>().bar = hpBar;
        }
    }

    private void OnStateChanged(IState state)
    {
        if (statesText != null)
        {
            var name = state.GetType().Name;
            statesText.text = name;
        }
    }

    void DamageStamina()
    {
        LeanTween.cancel(staminaBar.fillRect);
        LeanTween.cancel(staminaBarEase.fillRect);
        //Brings up the staminaBar in 0.1 seconds
        LeanTween.alpha(staminaBarEase.fillRect, 1f, 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);
        LeanTween.alpha(staminaBar.fillRect, 1f, 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);

        //new desired value for stamina bar
        staminaBar.value = actorData.currentStamina / actorData.maxStamina;


        LeanTween.value(staminaBarEase.value, staminaBar.value, 2f).setEaseOutCubic().setIgnoreTimeScale(true).setOnComplete(() =>
        {
            LeanTween.alpha(staminaBarEase.fillRect, 0f, 10f).setEaseOutExpo().setIgnoreTimeScale(true);
            LeanTween.alpha(staminaBar.fillRect, 0f, 10f).setEaseOutExpo().setIgnoreTimeScale(true);
        }).setOnUpdate((float val) =>
        {
            staminaBarEase.value = val;
        });
        //instat decrease stamina
        //wait a bit 
        //lerp ease
        //wait a bit
        //dissapear
    }

    internal void Reset()
    {
        actorData = actor.ActorData;
    }

    void HealStamina()
    {
        LeanTween.cancel(staminaBar.fillRect);
        LeanTween.cancel(staminaBarEase.fillRect);
        //Brings up the staminaBar in 0.1 seconds
        LeanTween.alpha(staminaBarEase.fillRect, 1f, 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);
        LeanTween.alpha(staminaBar.fillRect, 1f, 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);
        float newvalue = actorData.currentStamina / actorData.maxStamina;
        //new desired value for stamina bar
        LeanTween.value(staminaBar.value, newvalue, 1f).setEaseOutCubic().setIgnoreTimeScale(true).setOnUpdate((float val) =>
        {
            staminaBar.value = val;
            staminaBarEase.value = val;
        });
    }



    // Update is called once per frame
    void Update()
    {
        if (lastStamina < actorData.currentStamina)
        {
            //heal stamina
            HealStamina();
        }
        else if (lastStamina > actorData.currentStamina)
        {
            //damage stamina
            DamageStamina();
        }
        lastStamina = actor.ActorData.currentStamina;

        hpBar.value = actorData.GetCurrentHP() / actorData.maxHp;
        hpBarEase.value = Mathf.Lerp(hpBarEase.value, hpBar.value, 0.01f);
        //hpBarText.text = $"{ actorData.currentHp}/{actorData.maxHp}";

        postureBar.value = actorData.currentPosture / actorData.maxPosture;
        postureBarEase.value = Mathf.Lerp(postureBarEase.value, postureBar.value, 0.01f);
        //postureBarText.text = $"{ actorData.currentPosture}/{actorData.maxPosture}";

        buildupBar.value = actorData.currentBuildup / actorData.maxBuildup;
        buildupBarEase.value = Mathf.Lerp(buildupBarEase.value, buildupBar.value, 0.01f);




        //if (buildupBar.value != 0)
        //    buildupBarText.text = $"{ actorData.currentBuildup}/{actorData.maxBuildup}";
        //else
        //    buildupBarText.text = string.Empty;

    }

    public string WriteStatusTypes(List<BaseStatus> statusLists)
    {
        System.Text.StringBuilder result = new System.Text.StringBuilder();
        foreach (BaseStatus status in statusLists)
        {
            result.Append(status.GetType().Name).Append(", ");
        }
        // Remove the last ", " if there's any content in the result
        if (result.Length > 0)
        {
            result.Length -= 2;
        }
        return result.ToString();
    }
}
