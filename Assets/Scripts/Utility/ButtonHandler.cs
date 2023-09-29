using Assets;
using Assets.Scripts.Actions;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI remainingUses;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private TextMeshProUGUI range;
    [SerializeField] private TextMeshProUGUI damage;
    [SerializeField] private TextMeshProUGUI postureDamage;
    [SerializeField] private TextMeshProUGUI knockback;
    [SerializeField] private TextMeshProUGUI tags;


    public BaseAction referencedAction;
    public BaseSkill referencedSkill;
    public BaseReaction referencedReaction;

    public static void KillAll()
    {
        var skillButtons = GameObject.FindGameObjectsWithTag("SkillButton");
        foreach (var button in skillButtons)
        {
            Destroy(button);
        }
    }

    public void Init()
    {
        if (referencedAction != null) SetUIFromAction(referencedAction);
        if (referencedSkill != null) SetUIFromAction(referencedSkill);
        if (referencedReaction != null) SetUIFromAction(referencedReaction);
    }

    public void InitCard()
    {
        skillName.text = referencedSkill.Name;
        string plusSymbol = "+";
        remainingUses.text = referencedSkill.TotalUses != 0 ? ConcatWithPlus(plusSymbol, referencedSkill.TotalUses) : string.Empty;
        speed.text += referencedSkill.Speed;
        range.text += referencedSkill.Range;
        damage.text += referencedSkill.Damage;
        postureDamage.text += referencedSkill.PostureDamage;
        knockback.text += referencedSkill.KnockbackForce;
        tags.text = GenerateTagString(referencedSkill.Tags);
    }

    private string GenerateTagString(List<SKILLTAG> tags)
    {
        string tagString = string.Empty;
        foreach (SKILLTAG tag in tags)
        {
            switch (tag)
            {
                case SKILLTAG.MOVE_NEAR_ENEMY_BEFORE_ATTACK:
                    tagString += "Moves Near Enemy" + Environment.NewLine;
                    break;
                case SKILLTAG.PROJECTILE:
                    break;
                case SKILLTAG.KNOCKBACK_AIR:
                    tagString += "Launches in Air" + Environment.NewLine;
                    break;
                case SKILLTAG.MOVE_OFFSET_BEHIND:
                    tagString += "Knock Towards Camera" + Environment.NewLine;
                    break;
                case SKILLTAG.MOVE_OFFSET_INFRONT:
                    tagString += "Knock Away From Camera" + Environment.NewLine;
                    break;
                case SKILLTAG.NO_REACTION:
                    tagString += "Enemy Cannot React" + Environment.NewLine;
                    break;
                case SKILLTAG.REPEAT_TURN:
                    tagString += "Act Again On Completion" + Environment.NewLine;
                    break;
                default:
                    break;
            }
        }
        return tagString;
    }

    private void SetUIFromAction(BaseAction action)
    {
        if (action != null)
        {
            skillName.text = action.Name;
            string plusSymbol = "+";
            remainingUses.text = action.TotalUses != 0 ? ConcatWithPlus(plusSymbol, action.remainingUses) : string.Empty;
            SetButtonInteractable(action);
        }
    }

    public void SetRemainingUsesText(string text)
    {
        remainingUses.text = text;
    }

    private string ConcatWithPlus(string symbol, int value)
    {
        return new string(symbol[0], value);
    }

    private void SetButtonInteractable(BaseAction action)
    {
        GetComponent<UnityEngine.UI.Button>().interactable = action.HasUsesLeft();

    }

    public void SetButtonInteractable(bool interactable)
    {
        GetComponent<UnityEngine.UI.Button>().interactable = interactable;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.1f);
    }
}
