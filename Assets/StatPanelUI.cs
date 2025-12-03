using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Game;
using Assets.Scripts.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;

public class StatPanelUI : MonoBehaviour
{
    public TextMeshProUGUI PlayerName;
    public TextMeshProUGUI HPBars;
    public TextMeshProUGUI MaxStamina;
    public TextMeshProUGUI MaxBuildup;
    public TextMeshProUGUI MaxPosture;
    public TextMeshProUGUI STRENGTH;
    public TextMeshProUGUI AGILITY;
    public TextMeshProUGUI PERCEPTION;
    public TextMeshProUGUI SPIRIT;

    public ActorData actorData;

    public void Start()
    {
        actorData = GameFlowManager.instance.playerActor.ActorData;
    }

    private void Update()
    {
        if(actorData == null) return;
        PlayerName.text = actorData.Name;
        HPBars.text = GeneratedHPBarString(actorData.hpBars);
        MaxStamina.text = "<color=green>" + actorData.maxStamina.ToString();
        MaxBuildup.text = "<color=#5ACEFF>" + actorData.maxBuildup.ToString();
        MaxPosture.text = "<color=white>"+ actorData.maxPosture.ToString();
        STRENGTH.text = "STR: " + actorData.Strength;
        AGILITY.text = "AGI: " + actorData.Mind;
        PERCEPTION.text = "PER: " + actorData.Agility;
        SPIRIT.text = "SPI: " + actorData.Spirit;

    }
    private string GeneratedHPBarString(List<HpBar> bars)
    {
        var result = "";
        result += "<color=red>";
        foreach (var bar in bars)
        {
            result += $"[{bar.currentHp.ToString("0.#")}/{bar.maxHp.ToString("0.#")}]  ";
        }
        return result;
    }
}
