using Assets;
using Assets.Scripts.Actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject ButtonPrefab;
    [SerializeField]
    private TextMeshProUGUI TopTextbox; 
    [SerializeField]
    private TextMeshProUGUI Status;
    [SerializeField]
    private TextMeshProUGUI StoneSlab;
    private float fadeduration = 9999f;
    private float timer = 0f;
    private static UIManager instance;
    internal BaseAction selectedAction;
    internal BaseSkill selectedSkill;
    public bool waitingForAction = false;
    private Action onActionSelected;
    internal BaseReaction selectedReaction;

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
        
    }

    public void ChangeStatus(string status)
    {
        Status.text = status;
        StoneSlab.text += status + Environment.NewLine;
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

    public List<ButtonHandler> DrawActionsRadiallyOnScreenPoint(Vector2 point, List<BaseAction> actions)
    {
        float radius = 100f;
        var handlers = new List<ButtonHandler>();

        float anglestep = 360f / actions.Count;
        for(int i = 0; i < actions.Count; i++)
        {
            float angle = i * anglestep;
            float x = radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float y = radius * Mathf.Sin(Mathf.Deg2Rad * angle);
            Vector2 position = new Vector2(x, y);

            GameObject UISkill = Instantiate(ButtonPrefab, position + point, Quaternion.identity);
            UISkill.transform.SetParent(GameObject.FindGameObjectWithTag("MainCanvas").transform);
            UISkill.GetComponent<RectTransform>().position = point + position;
            var handler = UISkill.GetComponent<ButtonHandler>();
            handler.referencedAction = actions[i];
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
        if (!waitingForAction)
        {
            List<BaseAction> fakeActions = new List<BaseAction>();
            var activeChar = BattleManager.GetInstance().GetActiveActor();
            var skills = activeChar.GetBaseActor().skills;
            var originPoint = Camera.main.WorldToScreenPoint(activeChar.transform.position + Vector3.up * 0.5f);

            foreach (var skill in skills)
            {
                fakeActions.Add(skill);
            }

            var buttonHandlers = DrawActionsRadiallyOnScreenPoint(originPoint, fakeActions);
            for (int i = 0; i < skills.Count; i++)
            {
                buttonHandlers[i].referencedSkill = skills[i];
                buttonHandlers[i].referencedAction = null;
            }
            this.onActionSelected = onSkillSelected;
            this.waitingForAction = true;
        }
    }

    public void DrawAllActorReactions(BaseActorBattler actor, Action onReactionSelected)
    {
        if (!waitingForAction)
        {
            List<BaseAction> fakeReactions = new List<BaseAction>();
            var reactions = actor.GetBaseActor().reactions;
            var originPoint = Camera.main.WorldToScreenPoint(actor.transform.position + Vector3.up * 0.5f);

            foreach (var reaction in reactions)
            {
                fakeReactions.Add(reaction);
            }

            var buttonHandlers = DrawActionsRadiallyOnScreenPoint(originPoint, fakeReactions);
            for (int i = 0; i < reactions.Count; i++)
            {
                buttonHandlers[i].referencedReaction = reactions[i];
                buttonHandlers[i].referencedAction = null;
            }
            this.onActionSelected = onReactionSelected;
            this.waitingForAction = true;
        }
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

    //1. Attack or Move or Skill // MoveWithingRange if able;
    //2. Reaction Check > Give control to the enemy. Allow him to chose from his reactions (Skill used for mitigation damage)
    //3. Deal Dmg, Apply Effects, Check for Posture break
    ////If Posture break > go back to 1.
    //4. ChoseNextActivePlayer();
    //
}
