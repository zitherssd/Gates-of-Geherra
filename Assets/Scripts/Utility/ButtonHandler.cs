using Assets;
using Assets.Scripts.Actions;
using TMPro;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI SkillName;
    [SerializeField]
    private TextMeshProUGUI RemainingUses;
    [SerializeField]

    public BaseAction referencedAction;
    public BaseSkill referencedSkill;
    public BaseReaction referencedReaction;

    public static void KillAll()
    {
        var diers = GameObject.FindGameObjectsWithTag("SkillButton");
        foreach (var dier in diers)
        {
            Destroy(dier);
        }
    }

    public void OnClick()
    {
        KillAll();
        UIManager.GetInstance().selectedSkill = referencedSkill;
        UIManager.GetInstance().selectedReaction = referencedReaction;
        UIManager.GetInstance().CallbackActionSelected();
    }

    public void Start()
    {
        if (referencedAction != null)
        {
            SkillName.text = referencedAction.Name;
            string plussyombol = "+";

            RemainingUses.text = referencedAction.TotalUses != 0 ? Concatx(plussyombol, referencedAction.remainingUses) : string.Empty;
            
            
            if (referencedAction.TotalUses != 0 && referencedAction.remainingUses == 0)
                GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
        if (referencedSkill != null)
        {
            SkillName.text = referencedSkill.Name;
            string plussyombol = "+";

            RemainingUses.text = referencedSkill.TotalUses != 0 ? Concatx(plussyombol, referencedSkill.remainingUses) : string.Empty;
            if (referencedSkill.TotalUses != 0 && referencedSkill.remainingUses == 0)
                GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
        if (referencedReaction != null)
        {
            SkillName.text = referencedReaction.Name;
            string plussyombol = "+";

            RemainingUses.text = referencedReaction.TotalUses != 0 ? Concatx(plussyombol, referencedReaction.remainingUses) : string.Empty;
            if (referencedReaction.TotalUses != 0 && referencedReaction.remainingUses == 0)
                GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
    }

    private string Concatx(string concatee, int repeats)
    {
        var orig = concatee;
        var result = string.Empty;
        for (int i = 0; i < repeats; i++)
        {
            result += orig;
        }
        return result;
    }
}
