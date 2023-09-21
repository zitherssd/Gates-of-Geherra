using Assets;
using Assets.Scripts.Actions;
using TMPro;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI remainingUses;

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
        if(referencedAction != null) SetUIFromAction(referencedAction);
        if (referencedSkill != null)  SetUIFromAction(referencedSkill);
        if (referencedReaction != null)  SetUIFromAction(referencedReaction);
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

    public void OnClick()
    {
        KillAll();
        var uiManager = UIManager.GetInstance();
        uiManager.selectedSkill = referencedSkill;
        uiManager.selectedReaction = referencedReaction;
        uiManager.CallbackActionSelected();
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, 0.1f);
    }
}
