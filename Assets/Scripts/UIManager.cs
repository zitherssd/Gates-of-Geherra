using Assets;
using Assets.Scripts.Actions;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using Assets.Scripts.Battle;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private TextMeshProUGUI TopTextbox;
    [SerializeField] private TextMeshProUGUI MiddleTextbox;
    [SerializeField] private TextMeshProUGUI StoneSlab;
    [SerializeField] private TextMeshProUGUI TurnText;
    [SerializeField] private UnityEngine.UI.Image fadeImage;
    [Range(0, 1)] public float letterPause = 0.01f;
    [Range(0, 1)] public float fadeSpeed;
    public AudioClip typeSound1;
    public AudioClip typeSound2;

    [HideInInspector] public BaseReaction selectedReaction;
    [HideInInspector] public BaseAction selectedAction;
    [HideInInspector] public BaseSkill selectedSkill;

    private static UIManager instance;
    private float fadeduration = 1;
    private float timer = 0f;
    private bool waitingForAction = false;
    private Action onActionSelected;
    public GameObject OriginPoint;
    public GameObject ActionsHolder;
    public GameObject ActionSlot;
    public UnityEngine.UI.Button EndButton;
    public GameObject Slider;
    public GameObject Knob;

    public Action<Assets.BaseAction> OnActionSelected { get; private set; }
    public Action<BaseReaction> OnReactionSelected { get; private set; }

    public static UIManager GetInstance()
    {
        return instance;
    }
    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        OriginPoint = GameObject.FindGameObjectWithTag("OriginPoint");

        EndButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate ()
        {
            if (ActionSlot.transform.childCount > 0)
            {
                if(OnActionSelected != null)
                {
                    OnActionSelected(ActionSlot.GetComponentInChildren<ButtonHandler>().referencedAction);
                    OnActionSelected = null;
                }
            }
            else
            {
                if(OnActionSelected != null)
                {
                    OnActionSelected(null);
                    OnActionSelected = null;
                }
            }
            ButtonHandler.KillAll();
            EndButton.gameObject.SetActive(false);
            ActionSlot.SetActive(false);
            Slider.SetActive(false);
            Knob.SetActive(false);
            LeanTween.scale(ActionsHolder, new Vector3(1, 0, 1), 0.1f).setEaseOutCubic().setIgnoreTimeScale(true);
        });
    }

    public void ChangeStatus(string status)
    {
        TopTextbox.text = status;
    }

    public void AddToStoneSlab(string textToAdd)
    {
        var previous = StoneSlab.text;
        StoneSlab.text = textToAdd + Environment.NewLine;
        StoneSlab.text += previous;
    }

    public List<ButtonHandler> DrawActionsRadiallyOnScreenPoint(List<Assets.BaseAction> actions)
    {
        LeanTween.scale(ActionsHolder, Vector3.one, 0.2f).setEaseOutCubic().setIgnoreTimeScale(true);
        float radius = 100f;
        var handlers = new List<ButtonHandler>();

        float anglestep = 360f / actions.Count;
        for (int i = 0; i < actions.Count; i++)
        {
            float angle = i * anglestep;
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float y = radius * Mathf.Sin(Mathf.Deg2Rad * angle);
            Vector2 position = new Vector2(x, y);

            GameObject UISkill = Instantiate(buttonPrefab, position, Quaternion.identity);
            UISkill.transform.SetParent(ActionsHolder.transform);
            UISkill.GetComponent<RectTransform>().localPosition = position;
            var handler = UISkill.GetComponent<ButtonHandler>();
            handler.referencedAction = actions[i];
            handler.Init();
            handlers.Add(handler);
        }
        EndButton.gameObject.SetActive(true);
        ActionSlot.SetActive(true);
        return handlers;
    }

    public void CancelAll()
    {
        ButtonHandler.KillAll();
        selectedAction = null;
        selectedSkill = null;
        selectedReaction = null;
        waitingForAction = false;
        onActionSelected = null;
    }

    public void DrawActiveActorSkills(Action onSkillSelected)
    {

        List<Assets.BaseAction> fakeActions = new List<Assets.BaseAction>();
            var activeChar = BattleManager.instance.GetActiveActor();
            var skills = activeChar.ActorData.actions;

            foreach (var skill in skills)
            {
                fakeActions.Add(skill);
            }

            var buttonHandlers = DrawActionsRadiallyOnScreenPoint(fakeActions);
            for (int i = 0; i < skills.Count; i++)
            {
                var invalidReason = "";

                buttonHandlers[i].referencedAction = null;
                buttonHandlers[i].SetButtonInteractable(skills[i].IsValid(activeChar, out invalidReason));
                if (invalidReason != string.Empty) buttonHandlers[i].SetRemainingUsesText(invalidReason);
            }
            this.onActionSelected = onSkillSelected;
            this.waitingForAction = true;
    }

    public void DrawAllActorReactions(Actor actor, Action onReactionSelected)
    {
        if (!waitingForAction)
        {
            List<Assets.BaseAction> fakeReactions = new List<Assets.BaseAction>();
            var reactions = actor.ActorData.reactions;

            foreach (var reaction in reactions)
            {
                fakeReactions.Add(reaction);
            }

            var buttonHandlers = DrawActionsRadiallyOnScreenPoint(fakeReactions);
            for (int i = 0; i < reactions.Count; i++)
            {
                buttonHandlers[i].referencedReaction = reactions[i];
                buttonHandlers[i].referencedAction = null;
            }
            this.onActionSelected = onReactionSelected;
            this.waitingForAction = true;
        }
    }
    


    internal void DrawActionAboveHead(Actor actor, BaseAction action)
    {
            GameObject drawnSkill = Instantiate(buttonPrefab, actor.transform.position, Quaternion.identity);
            drawnSkill.transform.SetParent(actor.originPointInUI.transform);
            drawnSkill.transform.localPosition = Vector3.zero;
            drawnSkill.GetComponent<RectTransform>().localPosition = Vector3.zero;
            var handler = drawnSkill.GetComponent<ButtonHandler>();
            handler.referencedAction = action;
            handler.Init();
            handler.SetDraggable(false);
            LeanTween.scale(actor.originPointInUI.transform.gameObject, new Vector3(1, 1, 1), 0.3f).setEaseOutBack().setIgnoreTimeScale(true);
    }

    internal void KillActionAboveHead(Actor actor)
    {
        var obj = actor.originPointInUI.transform;
        if (obj.childCount > 0)
        {
            LeanTween.scale(obj.gameObject, new Vector3(1, 0, 1), 0.3f).setEaseOutBack().setIgnoreTimeScale(true).setOnComplete(() => { Destroy(obj.GetChild(0).gameObject); });
        }
        else
            LeanTween.scale(obj.gameObject, new Vector3(1, 0, 1), 0.3f).setEaseOutBack().setIgnoreTimeScale(true);
    }

    public void DrawAllActorReactionsAboveSpeed(Actor actor, int minimumSpeed, Action onReactionSelected)
    {

        List<Assets.BaseAction> fakeReactions = new List<Assets.BaseAction>();
            List<BaseReaction> chosenReactions = new List<BaseReaction>();
            var reactions = actor.ActorData.reactions;


            foreach (var reaction in reactions)
            {
                if (reaction.Speed >= minimumSpeed)
                    fakeReactions.Add(reaction);
                chosenReactions.Add(reaction);
            }

            var buttonHandlers = DrawActionsRadiallyOnScreenPoint(fakeReactions);
            for (int i = 0; i < fakeReactions.Count; i++)
            {
                buttonHandlers[i].referencedReaction = chosenReactions[i];
                buttonHandlers[i].referencedAction = null;
            }
            this.onActionSelected = onReactionSelected;
            this.waitingForAction = true;
    }

    public void DrawActionsAndWaitForSelectionOrNull(List<Assets.BaseAction> ActionsToDraw, Action<BaseAction> onActionSelected)
    {
        var buttonHandlers = DrawActionsRadiallyOnScreenPoint(ActionsToDraw);
        
        for (int i = 0; i < ActionsToDraw.Count; i++)
        {
            buttonHandlers[i].referencedAction = ActionsToDraw[i];
            buttonHandlers[i].Click = onActionSelected;
        }
        UIManager.instance.EndButton.onClick.RemoveAllListeners();
        UIManager.instance.EndButton.onClick.AddListener(() =>
        {
            ButtonHandler.KillAll();
            onActionSelected(null);
        });
    }


    public void ReturnNullSkill(Vector2 position)
    {
        InputHandler.instance.OnHold -= ReturnNullSkill;
        ButtonHandler.KillAll();
        this.OnActionSelected(null);
    }




    public static Vector2 rotate(Vector2 v, float delta)
    {
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    public void SetText(string text)
    {
        TopTextbox.text = text;

    }

    internal void SetTextThenFade(string texttobeshown, float fadeduration)
    {
        TopTextbox.text = texttobeshown;
        TopTextbox.color = new Color(1, 1, 1, 1);
        this.fadeduration = fadeduration;
        timer = 0f;
    }

    public IEnumerator TypeTextMiddleLetterByLetter(string text, Action onTypingComplete)
    {
        MiddleTextbox.color = new Color(MiddleTextbox.color.r, MiddleTextbox.color.g, MiddleTextbox.color.b, 1);
        foreach (char letter in text.ToCharArray())
        {
            MiddleTextbox.text += letter;
            if (typeSound1 && typeSound2)
                SoundManager.instance.RandomizeSfx(typeSound1, typeSound2);
            yield return 0;
            yield return new WaitForSeconds(letterPause);
        }
        onTypingComplete();
    }

    public IEnumerator FadeMiddleText(float fadeDuration)
    {
        Color originalColor = MiddleTextbox.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Fade to transparent

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            MiddleTextbox.color = Color.Lerp(originalColor, targetColor, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the target color is reached
        MiddleTextbox.color = targetColor;
        MiddleTextbox.text = string.Empty;
    }

    public void Fade(bool fadeIn, Action onFadeComplete)
    {
        if (!fadeIn)
            LeanTween.alpha(fadeImage.rectTransform, 0f, 0.5f).setOnComplete(onFadeComplete);
        else
            LeanTween.alpha(fadeImage.rectTransform, 1f, 0.5f).setOnComplete(onFadeComplete);
    }

    public void SetTurn(uint turn)
    {
        TurnText.text = "Turn " + turn.ToString();
    }

    public  void HideUI()
    {
        ActionsHolder.transform.parent.gameObject.SetActive(false);
    }
    public  void ShowUI()
    {
        ActionsHolder.transform.parent.gameObject.SetActive(true);

    }

    //1. Attack or Move or Skill // MoveWithingRange if able;
    //2. Reaction Check > Give control to the enemy. Allow him to chose from his reactions (Skill used for mitigation damage)
    //3. Deal Dmg, Apply Effects, Check for Posture break
    ////If Posture break > go back to 1.
    //4. ChoseNextActivePlayer();
    //
}
