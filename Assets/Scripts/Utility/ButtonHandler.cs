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
    private Button button;
    public BaseAction referencedAction;
    public Action<BaseAction> Click;
    public bool Disabled = false;

    private Vector2 targetPos;


    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;

    }

    public static void KillAll()
    {
        int childCount = actionHolder.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = actionHolder.transform.GetChild(i);
            Destroy(child.gameObject);
        }


        childCount = skillHolder.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = skillHolder.transform.GetChild(i);
            Destroy(child.gameObject);
        }

    }

    private void Awake()
    {
        button = gameObject.GetComponent<Button>();
        if (actionHolder == null) actionHolder = GameObject.Find("LeftContainer");
        if (skillHolder == null) skillHolder = GameObject.Find("RightContainer");
        //if (actionSlot == null) actionSlot = GameObject.Find("ActionSlot");
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
        string plusSymbol = "+";
        if (referencedAction.TotalUses != 0 && referencedAction.remainingUses > 0)
        {
            remainingUses.text = ConcatWithPlus(plusSymbol, referencedAction.remainingUses);
        }

        else
        {
            remainingUses.text = string.Empty;
        }

        SetInteractable(referencedAction.IsValid(player, out _));

        if(referencedAction)
        cooldownimage.fillAmount = Mathf.Clamp(referencedAction.currentCooldownTimer / referencedAction.CooldownTimer, 0, 1);

        if (Disabled) return;
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
            deltaScaled = new Vector2(StaticHelpers.LinearMap(delta.x, 0, 300, 0, 1), StaticHelpers.LinearMap(delta.y, 0, 300, 0, 1));
            deltaScaled = Vector2.ClampMagnitude(deltaScaled, 1);
            if (referencedAction.Tags.Contains(TAG.USESTICK) || referencedAction.Type == BUTTONTYPE.VECTOR || referencedAction.Type == BUTTONTYPE.CONTINUOUS_VECTOR)
            {
                //Show guide and store Direction in skill;
                guide.transform.position = player.transform.position + GetRelativeToCamera(deltaScaled * referencedAction.StickMult);
                referencedAction.Direction = GetRelativeToCamera(deltaScaled);
            }

            rect.transform.position = Vector2.Lerp(rect.transform.position, targetPos + deltaScaled * 150f, 0.2f);

        }
    }

    public void Initialize(BaseAction referencedAction) //This can be moved in Start probably
    {
        if (referencedAction != null)
        {
            SetUIFromAction(referencedAction);
            SetInteractable(referencedAction.IsValid(BattleManager.instance.PlayerActors[0], out _));
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


            SetInteractable(action);
        }
    }

    private string ConcatWithPlus(string symbol, int value)
    {
        return new string(symbol[0], value);
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
        isPressed = false;
        LeanTween.cancel(rect);
        LeanTween.move(rect, homePosition, 0.15f).setEaseOutCubic().setIgnoreTimeScale(true);
        LeanTween.scale(rect, Vector3.one, 0.5f).setEaseOutCubic().setIgnoreTimeScale(true);
        guide.GetComponent<ParticleSystem>().Stop();

    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (Disabled) return;

        if (button.IsInteractable() == false) return;

        if (referencedAction is AttackSkill)
        {
            CameraManager.instance.SlowTrack = false;
        }

        homePosition = gameObject.transform.position;

        switch (referencedAction.Type)
        {
            case BUTTONTYPE.INSTANT:
                //UseSkill nothing else
                return;
                break;
            case BUTTONTYPE.VECTOR:
                UIManager.instance.HideAllButThis(referencedAction);
                MoveToCenter();
                UIManager.instance.GainMeter(referencedAction.SlowDownMeterGainOnPress);
                isPressed = true;
                pointerDownPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                break;
            case BUTTONTYPE.CONTINNUOUS:
                HideUIExceptThisAndUseAction();
                break;
            case BUTTONTYPE.CONTINUOUS_VECTOR:
                MoveToCenter();
                isPressed = true;
                pointerDownPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                HideUIExceptThisAndUseAction();
                break;
            default:
                break;
        }

        //Output the name of the GameObject that is being clicked
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        if (button.IsInteractable() == false) return;
        isPressed = false;
        if (Disabled) return;

        switch (referencedAction.Type)
        {
            case BUTTONTYPE.INSTANT:
                HideUIAndUseAction();
                break;
            case BUTTONTYPE.VECTOR:
                HideUIAndUseAction();
                MoveToHome();
                break;
            case BUTTONTYPE.CONTINNUOUS:
                referencedAction.cancel?.Invoke();
                break;
            case BUTTONTYPE.CONTINUOUS_VECTOR:
                referencedAction.cancel?.Invoke();
                UIManager.instance.GainMeter(referencedAction.SlowdownMeterGainOnRelease);
                MoveToHome();
                break;
            default:
                break;
        }
        isPressed = false;
    }

    public void HideUIAndUseAction()
    {
        isPressed = false;
        UIManager.instance.HideUI();
        player.UseAction(referencedAction, () => { player.state.TransitionTo(player.state.idleState); });
    }
    public void HideUIExceptThisAndUseAction()
    {
        UIManager.instance.HideAllButThis(referencedAction);
        player.UseAction(referencedAction, () => { player.state.TransitionTo(player.state.idleState); });
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

    private RectTransform rect;
    private bool isPressed;
    private Vector2 pointerDownPosition;
    private Vector3 homePosition;
    private Vector2 deltaScaled;
}
