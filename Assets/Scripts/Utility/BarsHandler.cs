using Assets;
using Assets.Scripts.Battle.States;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarsHandler : MonoBehaviour
{
    [SerializeField]
    private BaseActorBattler actor;
    private Actor actorData;
    [SerializeField]
    private Slider hpBar;
    [SerializeField]
    private Slider hpBarEase;
    [SerializeField] 
    private TextMeshProUGUI hpBarText;
    [SerializeField]
    private Slider postureBar;
    [SerializeField]
    private Slider postureBarEase;
    [SerializeField]
    private TextMeshProUGUI postureBarText;
    [SerializeField]
    private Slider buildupBar;
    [SerializeField]
    private Slider buildupBarEase;
    [SerializeField]
    private TextMeshProUGUI buildupBarText;
    [SerializeField] TextMeshProUGUI statesText;
    // Start is called before the first frame update
    void Start()
    {
        actor = gameObject.GetComponentInParent<BaseActorBattler>();
    }

    // Update is called once per frame
    void Update()
    {
        actorData = actor.Actor;

        hpBar.value = actorData.currentHp / actorData.maxHp;
        hpBarEase.value = Mathf.Lerp(hpBarEase.value, hpBar.value, 0.01f);
        //hpBarText.text = $"{ actorData.currentHp}/{actorData.maxHp}";

        postureBar.value = actorData.currentPosture / actorData.maxPosture;
        postureBarEase.value = Mathf.Lerp(postureBarEase.value, postureBar.value, 0.01f);
        //postureBarText.text = $"{ actorData.currentPosture}/{actorData.maxPosture}";

        buildupBar.value = actorData.currentBuildup / actorData.maxBuildup;
        buildupBarEase.value = Mathf.Lerp(buildupBarEase.value, buildupBar.value, 0.01f);
        statesText.text = WriteStatusTypes(actor.activeStates);
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
