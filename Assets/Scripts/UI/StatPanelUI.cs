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
    public TextMeshProUGUI MIND;
    public TextMeshProUGUI SPIRIT;

    public ActorRuntime actorData;

    private void OnEnable()
    {
        GameSession.Instance.OnPlayerSpawned += SetPlayer;
    }

    private void OnDisable()
    {
        if (GameSession.Exists)
            GameSession.Instance.OnPlayerSpawned -= SetPlayer;
    }

    public void Start()
    {
        // Read the persistent run model directly so the panel works in any scene (arena or rest)
        // without depending on GameFlowManager; OnPlayerSpawned refreshes it when a body spawns.
        if (GameSession.Exists && GameSession.Instance.PlayerRuntime != null)
            actorData = GameSession.Instance.PlayerRuntime;
        else if (GameFlowManager.instance != null && GameFlowManager.instance.playerActor != null)
            actorData = GameFlowManager.instance.playerActor.Runtime;
    }

    private void SetPlayer(Actor player)
    {
        if (player != null)
            actorData = player.Runtime;
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
        AGILITY.text = "AGI: " + actorData.Agility;
        MIND.text = "MND: " + actorData.Mind;
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
