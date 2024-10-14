using Assets;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.BaseAction;

public class ButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{


    public Image image;
    public Image cooldownimage;

    private Transform home;
    public BaseAction referencedAction;
    public Action<BaseAction> Click;

    private Vector2 targetPos;



    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;

        gameObject.GetComponent<Button>().interactable = draggable;
        //animation
        if (gameObject.GetComponent<Button>().IsInteractable())
        {

        }

    }

    public static void KillAll()
    {
        int childCount = actionHolder.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = actionHolder.transform.GetChild(i);
            Destroy(child.gameObject);
        }
        if (actionSlot.transform.childCount > 0)
            Destroy(actionSlot.transform.GetChild(0).gameObject);

        childCount = skillHolder.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = skillHolder.transform.GetChild(i);
            Destroy(child.gameObject);
        }
        if (actionSlot.transform.childCount > 0)
            Destroy(actionSlot.transform.GetChild(0).gameObject);
    }

    private void Awake()
    {
        if (actionHolder == null) actionHolder = GameObject.Find("ActionHolder");
        if (skillHolder == null) skillHolder = GameObject.Find("SkillHolder");
        if (actionSlot == null) actionSlot = GameObject.Find("ActionSlot");
        if (actionSlotScript == null) actionSlotScript = actionSlot.GetComponent<ActionSlot>();
        if (guide == null) guide = GameObject.Find("PlacementGuide");
    }

    private void Start()
    {
        if (player == null) player = BattleManager.instance.PlayerActors[0];
        rect = gameObject.GetComponent<RectTransform>();
        home = transform.parent;
        LeanTween.scale(gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
    }

    private void Update()
    {
        if (referencedAction)
        {
            if (referencedAction.CooldownTimer == 0)
                cooldownimage.fillAmount = 0f;
            else
                cooldownimage.fillAmount = Mathf.Clamp(referencedAction.currentCooldownTimer / referencedAction.CooldownTimer, 0, 1);

            if (cooldownimage.fillAmount == 0) SetDraggable(referencedAction.IsValid(BattleManager.instance.PlayerActors[0], out _));
        }
        if (!isPressed) return;
        // Check if the pointer is currently pressed down
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            Vector2 currentPointerPosition;

            // Check if there is touch input
            if (Input.touchCount > 0)
            {
                // Use the position of the first touch
                currentPointerPosition = Input.GetTouch(0).position;
            }
            else
            {
                // Use the position of the mouse pointer
                currentPointerPosition = (Vector2)Input.mousePosition;
            }

            Vector2 delta = currentPointerPosition - pointerDownPosition; // Calculate the movement delta
            if (delta.magnitude > 300) { pointerDownPosition += Vector2.ClampMagnitude(delta, delta.magnitude - 300); }
            delta = Vector2.ClampMagnitude(delta, 300);
            deltaScaled = new Vector2(LinearMap(delta.x, 0, 300, 0, 1), LinearMap(delta.y, 0, 300, 0, 1));
            deltaScaled = Vector2.ClampMagnitude(deltaScaled, 1);
            if (referencedAction.Tags.Contains(TAG.USESTICK))
            {
                //Show guide and store Direction in skill;
                guide.transform.position = player.transform.position + GetRelativeToCamera(deltaScaled * referencedAction.StickMult);
                referencedAction.Direction = GetRelativeToCamera(deltaScaled);
            }

            rect.transform.position = Vector2.Lerp(rect.transform.position, targetPos + deltaScaled * 150f, 0.2f);


            if (deltaScaled.magnitude > 0.1f)
            {
                rect.transform.localScale = Vector3.Lerp(rect.transform.localScale, Vector3.one * LinearMap(deltaScaled.magnitude, 0.1f, 1, 1.0f, 1.5f), 0.1f);
            }
            else
            {
                rect.transform.localScale = Vector3.Lerp(rect.transform.localScale, Vector3.one * 0.66f, 0.1f);
            }

        }
    }

    public void Init() //This can be moved in Start probably
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
        if (referencedAction is AttackSkill)
        {
            var skill = referencedAction as AttackSkill;
            speed.text += skill.Speed;
            range.text += skill.Range;
            damage.text += skill.Damage;
            postureDamage.text += skill.PostureDamage;
            knockback.text += skill.KnockbackForce;
            if (referencedAction.Tags != null && referencedAction.Tags.Count > 0)
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
                case TAG.FREE:
                    tagString += "[FREE]";
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
            staminaCost.text = action.StaminaCost != 0 ? action.StaminaCost.ToString() : string.Empty;
            tags.text = GenerateTagString(action.Tags);
            if (description)
            {
                if (String.IsNullOrEmpty(action.Description))
                {
                    description.text = action.Description;
                }
            }


            SetDraggable(action);
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

    private void MoveToCenter()
    {
        // Calculate the center position of the screen
        Vector2 centerPosition = new(Screen.width / 2f, Screen.height / 2f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        targetPos = CanvasHandler.instance.GetComponent<RectTransform>().anchoredPosition;
        //LeanTween.move(rect, CanvasHandler.instance.GetComponent<RectTransform>().anchoredPosition, 0.15f).setEaseOutBack().setIgnoreTimeScale(true);
        LeanTween.scale(rect, Vector3.one * 1.5f, 1f).setEaseOutBack().setIgnoreTimeScale(true);
        if (referencedAction.Tags.Contains(TAG.USESTICK))
            guide.GetComponent<ParticleSystem>().Play();
    }

    private void MoveToHome()
    {
        LeanTween.cancel(rect);
        LeanTween.move(rect, homePosition, 0.15f).setEaseOutCubic().setIgnoreTimeScale(true);
        LeanTween.scale(rect, Vector3.one, 0.5f).setEaseOutCubic().setIgnoreTimeScale(true);
        guide.GetComponent<ParticleSystem>().Stop();

    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (referencedAction is AttackSkill)
        {
            CameraManager.instance.SlowTrack = false;
        }
        if (isDraggable == false) return;
        homePosition = gameObject.transform.localPosition;
        MoveToCenter();
        isPressed = true;
        pointerDownPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        //Output the name of the GameObject that is being clicked
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        if (isDraggable == false) return;
        MoveToHome();
        isPressed = false;
        if (deltaScaled.magnitude > 0.1f)
        {
            Click.Invoke(referencedAction);
            KillAll();
        }
    }

    float LinearMap(float input, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        return outputMin + (outputMax - outputMin) * ((input - inputMin) / (inputMax - inputMin));
    }

    public Vector3 GetRelativeToCamera(Vector2 direction)
    {
        var camera = Camera.main;
        var forward = camera.transform.forward; forward.y = 0;
        var right = camera.transform.right; right.y = 0;
        forward.Normalize(); right.Normalize();

        var desiredMoveDirection = forward * direction.y + right * direction.x;
        return desiredMoveDirection;
    }

    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI remainingUses;
    [SerializeField] private TextMeshProUGUI buildupCost;
    [SerializeField] private TextMeshProUGUI staminaCost;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private TextMeshProUGUI range;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI damage;
    [SerializeField] private TextMeshProUGUI postureDamage;
    [SerializeField] private TextMeshProUGUI knockback;
    [SerializeField] private TextMeshProUGUI tags;
    private static Actor player;
    private static GameObject actionHolder;
    private static GameObject skillHolder;
    private static GameObject actionSlot;
    private static ActionSlot actionSlotScript;
    private static GameObject guide;

    private bool isDraggable = true;
    private RectTransform rect;
    private bool isPressed;
    private Vector2 pointerDownPosition;
    private Vector3 homePosition;
    private Vector2 deltaScaled;
}
