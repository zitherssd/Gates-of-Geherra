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

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private TextMeshProUGUI TopTextbox;
    [SerializeField] private TextMeshProUGUI MiddleTextbox;
    [SerializeField] private TextMeshProUGUI StoneSlab;
    [SerializeField] private UnityEngine.UI.Image fadeImage;
    [Range(0, 1)] public float letterPause = 0.1f;
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



    public static UIManager GetInstance()
    {
        return instance;
    }
    public void Awake()
    {
        instance = this;
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

    private void Update()
    {
        if (selectedSkill != null)
            onActionSelected();
    }

    private void FadeOutText()
    {
        Color currentColor = TopTextbox.color;
        //float alpha = Mathf.Pow(1f - (timer / fadeduration), 2);
        float alpha = Mathf.Lerp(1f, 0f, (timer - fadeduration) / fadeduration);
        TopTextbox.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
    }

    public List<ButtonHandler> DrawActionsRadiallyOnScreenPoint(List<BaseAction> actions)
    {
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
            UISkill.transform.SetParent(GameObject.FindGameObjectWithTag("OriginPoint").transform);
            UISkill.GetComponent<RectTransform>().localPosition = position;
            var handler = UISkill.GetComponent<ButtonHandler>();
            handler.referencedAction = actions[i];
            handler.Init();
            handlers.Add(handler);
        }
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

            List<BaseAction> fakeActions = new List<BaseAction>();
            var activeChar = BattleManager.GetInstance().GetActiveActor();
            var skills = activeChar.GetBaseActor().skills;

            foreach (var skill in skills)
            {
                fakeActions.Add(skill);
            }

            var buttonHandlers = DrawActionsRadiallyOnScreenPoint(fakeActions);
            for (int i = 0; i < skills.Count; i++)
            {
                var invalidReason = "";

                buttonHandlers[i].referencedSkill = skills[i];
                buttonHandlers[i].referencedAction = null;
                buttonHandlers[i].SetButtonInteractable(skills[i].IsValid(activeChar, out invalidReason));
                if (invalidReason != string.Empty) buttonHandlers[i].SetRemainingUsesText(invalidReason);
            }
            this.onActionSelected = onSkillSelected;
            this.waitingForAction = true;
    }

    public void DrawAllActorReactions(BaseActorBattler actor, Action onReactionSelected)
    {
        if (!waitingForAction)
        {
            List<BaseAction> fakeReactions = new List<BaseAction>();
            var reactions = actor.GetBaseActor().reactions;

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

    public void DrawAllActorReactionsAboveSpeed(BaseActorBattler actor, int minimumSpeed, Action onReactionSelected)
    {

            List<BaseAction> fakeReactions = new List<BaseAction>();
            List<BaseReaction> chosenReactions = new List<BaseReaction>();
            var reactions = actor.GetBaseActor().reactions;


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





    public BaseSkill GetSelectedSkill()
    {
        var selTarget = selectedSkill;
        selectedSkill = null;
        ButtonHandler.KillAll();
        this.waitingForAction = false;
        return selTarget;
    }
    public BaseReaction GetSelectedReaction()
    {
        var selTarget = selectedReaction;
        selectedReaction = null;
        ButtonHandler.KillAll();
        this.waitingForAction = false;
        return selTarget;
    }
    public void CallbackActionSelected()
    {
        onActionSelected();
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

    public IEnumerator Fade(bool fadeIn, Action onFadeComplete)
    {
        var imgColor = fadeImage.color;
        float startAlpha = imgColor.a;
        float targetAlpha = fadeIn ? 1.0f : 0.0f;

        float elapsedTime = 0f;

        while (elapsedTime < fadeSpeed)
        {
            float t = elapsedTime / fadeSpeed;
            imgColor.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            fadeImage.color = imgColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        imgColor.a = targetAlpha;
        fadeImage.color = imgColor;

        onFadeComplete?.Invoke();
    }



    //1. Attack or Move or Skill // MoveWithingRange if able;
    //2. Reaction Check > Give control to the enemy. Allow him to chose from his reactions (Skill used for mitigation damage)
    //3. Deal Dmg, Apply Effects, Check for Posture break
    ////If Posture break > go back to 1.
    //4. ChoseNextActivePlayer();
    //
}
