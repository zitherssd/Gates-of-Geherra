using Assets;
using Assets.Scripts.Actions;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI remainingUses;
    [SerializeField] private TextMeshProUGUI buildupCost;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private TextMeshProUGUI range;
    [SerializeField] private TextMeshProUGUI damage;
    [SerializeField] private TextMeshProUGUI postureDamage;
    [SerializeField] private TextMeshProUGUI knockback;
    [SerializeField] private TextMeshProUGUI tags;
    [HideInInspector] public Transform parentafterDrag;
    public Image image;

    public BaseAction referencedAction;
    public BaseSkill referencedSkill;
    public BaseReaction referencedReaction;

    private bool isDraggable = true;

    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;
        var x = GetComponentsInChildren<Image>();

        if (isDraggable)
        {
            foreach (var y in x)
            {
                y.color = new Color(1, 1, 1, 1);
            }
            skillName.color = Color.black;
            remainingUses.color = Color.gray;
            buildupCost.color = Color.yellow;
        }
        else
        {
            foreach (var y in x)
            {
                y.color = new Color(0.05f, 0.05f, 0.05f, 0.1f);
            }
            skillName.color = new Color(skillName.color.r, skillName.color.g, skillName.color.b, 0.1f);
            remainingUses.color = new Color(remainingUses.color.r, remainingUses.color.g, remainingUses.color.b, 0.1f);
            buildupCost.color = new Color(buildupCost.color.r, buildupCost.color.g, buildupCost.color.b, 0.1f);
        }

    }

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
        if (referencedAction != null)
        {
            SetUIFromAction(referencedAction);
            SetDraggable(referencedAction.IsValid());
        }
        if (referencedSkill != null)
        {
            SetUIFromAction(referencedSkill);
            SetDraggable(referencedSkill.IsValid());
        }
        if (referencedReaction != null)
        {
            SetUIFromAction(referencedReaction);
            SetDraggable(referencedReaction.IsValid());
        }
    }

    public void InitCard()
    {
        skillName.text = referencedSkill.Name;
        buildupCost.text = referencedSkill.BuildupCost != 0 ? referencedSkill.BuildupCost.ToString() : string.Empty;

        string plusSymbol = "+";
        remainingUses.text = referencedSkill.TotalUses != 0 ? ConcatWithPlus(plusSymbol, referencedSkill.TotalUses) : string.Empty;
        speed.text += referencedSkill.Speed;
        range.text += referencedSkill.Range;
        damage.text += referencedSkill.Damage;
        postureDamage.text += referencedSkill.PostureDamage;
        knockback.text += referencedSkill.KnockbackForce;
        tags.text = GenerateTagString(referencedSkill.Tags);
    }

    private string GenerateTagString(List<TAG> tags)
    {
        string tagString = string.Empty;
        foreach (TAG tag in tags)
        {
            switch (tag)
            {
                case TAG.MOVE_NEAR_ENEMY_BEFORE_ATTACK:
                    tagString += "Moves Near Enemy" + Environment.NewLine;
                    break;
                case TAG.PROJECTILE:
                    break;
                case TAG.KNOCKBACK_AIR:
                    tagString += "Launches in Air" + Environment.NewLine;
                    break;
                case TAG.MOVE_OFFSET_BEHIND:
                    tagString += "Knock Towards Camera" + Environment.NewLine;
                    break;
                case TAG.MOVE_OFFSET_INFRONT:
                    tagString += "Knock Away From Camera" + Environment.NewLine;
                    break;
                case TAG.NO_REACTION:
                    tagString += "Enemy Cannot React" + Environment.NewLine;
                    break;
                case TAG.REPEAT_TURN:
                    tagString += "Act Again On Completion" + Environment.NewLine;
                    break;
                default:
                    break;
            }
        }
        return tagString;
    }

    private void SetUIFromAction(Assets.BaseAction action)
    {
        if (action != null)
        {
            skillName.text = action.Name;
            string plusSymbol = "+";
            remainingUses.text = action.TotalUses != 0 ? ConcatWithPlus(plusSymbol, action.remainingUses) : string.Empty;
            buildupCost.text = action.BuildupCost != 0 ? action.BuildupCost.ToString() : string.Empty;
            SetButtonInteractable(action);
            SetDraggable(action.IsValid() && BattleManager.GetInstance().GetActiveActor().Actor.currentBuildup >= action.BuildupCost);
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

    private void SetButtonInteractable(Assets.BaseAction action)
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        parentafterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;

        transform.SetParent(parentafterDrag);
        transform.position = Vector3.zero;
        image.raycastTarget = true;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());
    }
}
