using Assets;
using Assets.Scripts.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
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
    private static BaseActorBattler player;
    private static GameObject actionHolder;
    private static GameObject actionSlot;
    private static ActionSlot actionSlotScript;
    public Image image;

    private Transform home;
    public BaseAction referencedAction;
    public BaseSkill referencedSkill;
    public BaseReaction referencedReaction;

    private bool isDraggable = true;

    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;

        if (isDraggable)
        {
            remainingUses.color = Color.gray;
            buildupCost.color = Color.yellow;
        }
        else
        {
            remainingUses.color = new Color(remainingUses.color.r, remainingUses.color.g, remainingUses.color.b, 0.1f);
            buildupCost.color = new Color(buildupCost.color.r, buildupCost.color.g, buildupCost.color.b, 0.06f);
        }
        gameObject.GetComponent<Button>().interactable = draggable;
    }

    public static void KillAll()
    {
        int childCount = actionHolder.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = actionHolder.transform.GetChild(i);
            Destroy(child.gameObject);
        }
        if(actionSlot.transform.childCount > 0)
        Destroy(actionSlot.transform.GetChild(0).gameObject);
    }

    private void Awake()
    {
        if (player == null) player = BattleManager.instance.PlayerActors[0];
        if (actionHolder == null) actionHolder = GameObject.Find("ActionHolder");
        if (actionSlot == null) actionSlot = GameObject.Find("ActionSlot");
        if (actionSlotScript == null) actionSlotScript = actionSlot.GetComponent<ActionSlot>();
    }

    private void Start()
    {
        home = transform.parent;
        LeanTween.scale(gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
    }

    public void Init()
    {
        if (referencedAction != null)
        {
            SetUIFromAction(referencedAction);
            SetDraggable(referencedAction.IsValid(BattleManager.instance.PlayerActors[0], out _));
        }
    }

    public void InitCard()
    {
        skillName.text = referencedAction.Name;
        buildupCost.text = referencedAction.BuildupCost != 0 ? referencedAction.BuildupCost.ToString() : string.Empty;

        string plusSymbol = "+";
        remainingUses.text = referencedAction.TotalUses != 0 ? ConcatWithPlus(plusSymbol, referencedAction.TotalUses) : string.Empty;
        if(referencedAction is AttackSkill)
        {
            var skill = referencedAction as AttackSkill;
            speed.text += skill.Speed;
            range.text += skill.Range;
            damage.text += skill.Damage;
            postureDamage.text += skill.PostureDamage;
            knockback.text += skill.KnockbackForce;
            if(referencedAction.Tags != null && referencedAction.Tags.Count > 0)
            tags.text = GenerateTagString(referencedAction.Tags);
        }
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

    public void SetUIFromAction(Assets.BaseAction action)
    {
        if (action != null)
        {
            skillName.text = action.Name;
            string plusSymbol = "+";
            remainingUses.text = action.TotalUses != 0 ? ConcatWithPlus(plusSymbol, action.remainingUses) : string.Empty;
            buildupCost.text = action.BuildupCost != 0 ? action.BuildupCost.ToString() : string.Empty;
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

    private void SetButtonInteractable(Assets.BaseAction action)
    {
        GetComponent<UnityEngine.UI.Button>().interactable = false;
    }

    public void SetButtonInteractable(bool interactable)
    {
        GetComponent<UnityEngine.UI.Button>().interactable = interactable;
    }

    public void SetActive()
    {
        if (actionSlot.transform.childCount > 0)
        {
            var actionInSlot = actionSlot.transform.GetChild(0);

            if (actionInSlot == gameObject.transform) //this skill is already in the slot
            {
                gameObject.transform.SetParent(home.transform);
            }
            else
            {
                actionInSlot.SetParent(actionInSlot.GetComponent<ButtonHandler>().home.transform); //move action in slot back to home position
                gameObject.transform.SetParent(actionSlot.transform); //move this action inside the slot
                gameObject.transform.localScale = Vector3.zero;
                LeanTween.scale(gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
            }
        }
        else
        {
            gameObject.transform.SetParent(actionSlot.transform);
            gameObject.transform.localScale = Vector3.zero;
            LeanTween.scale(gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
        }
        actionSlotScript.OnDrop();
        //if actionslot is empty move to actionslot
        //if actionslot is full
        //  if this is already in actionslot move back to startingplace
        //  else replace action in actionslot with this one. move action in actionslot back home.
    }


    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    if (!isDraggable) return;

    //    parentafterDrag = transform.parent;
    //    transform.SetParent(transform.root);
    //    transform.SetAsLastSibling();
    //    image.raycastTarget = false;
    //    GetComponent<CanvasGroup>().blocksRaycasts = false;
    //}
    //public void OnDrag(PointerEventData eventData)
    //{
    //    if (!isDraggable) return;

    //    transform.position = eventData.position;
    //}
    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    if (!isDraggable) return;

    //    transform.SetParent(parentafterDrag);
    //    transform.position = Vector3.zero;
    //    image.raycastTarget = true;
    //    GetComponent<CanvasGroup>().blocksRaycasts = true;
    //    ExecuteEvents.ExecuteHierarchy<IHasChanged>(gameObject, null, (x, y) => x.HasChanged());
    //}
}
