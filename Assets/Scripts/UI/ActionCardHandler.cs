using Assets;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Actions;
using Assets.Scripts.Battle.Actions.Reactions;
using Assets.Scripts.Battle.Actions.Skills;
using TMPro;
using UnityEngine;

public class ActionCardHandler : MonoBehaviour
{
    private BaseAction _referencedAction;
    public BaseAction ReferencedAction { get { return _referencedAction; } set { _referencedAction = value; Initialize(); } }
    private static Actor playerActor;

    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI cooldown;
    [SerializeField] private TextMeshProUGUI staminaCost;
    [SerializeField] private TextMeshProUGUI buildupcost;
    [SerializeField] private TextMeshProUGUI uses;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private TextMeshProUGUI posture;
    [SerializeField] private TextMeshProUGUI buildupGain;



    public void Initialize()
    {
        skillName.text = ReferencedAction.Name.ToUpper();
        description.text = ReferencedAction.Description?.ToUpper();
        cooldown.text = ReferencedAction.CooldownTimer.ToString();
        if (ReferencedAction.BuildupGain != 0) buildupGain.text = "+" + ReferencedAction.BuildupGain.ToString() + " buildup".ToUpper(); else buildupGain.text = string.Empty;
        if (ReferencedAction.StaminaCost != 0) staminaCost.text = ReferencedAction.StaminaCost.ToString(); else staminaCost.text = string.Empty;
        if (ReferencedAction.BuildupCost != 0) buildupcost.text = ReferencedAction.BuildupCost.ToString(); else buildupcost.text = string.Empty;
        if (ReferencedAction.TotalUses != 0)
        {
            if (ReferencedAction.Tags.Contains(BaseAction.TAG.RECHARGE_TOTAL_USES))
                uses.text = $"recharges {ReferencedAction.TotalUses} uses".ToUpper();
            else
                uses.text = $"{ReferencedAction.TotalUses} total uses".ToUpper();
        }
        else uses.text = string.Empty;
        if(ReferencedAction is Charge)
            power.text = (ReferencedAction as Charge).power.ToString() + " power".ToUpper();
        if (ReferencedAction is Jump)
            power.text = (ReferencedAction as Jump).power.ToString() + " power".ToUpper();
        if (ReferencedAction is Dodge)
            power.text = (ReferencedAction as Dodge).force.ToString() + " force".ToUpper();
        if (ReferencedAction is AttackSkill)
        {
            power.text = (ReferencedAction as AttackSkill).Damage.ToString() + " damage".ToUpper();
            posture.text = (ReferencedAction as AttackSkill).PostureDamage.ToString() + " posture damage".ToUpper();
            buildupGain.text = "+" + (ReferencedAction as AttackSkill).BuildupGainOnHit.ToString() + " buildup".ToUpper();
            if (ReferencedAction.Tags.Contains(BaseAction.TAG.TECH)) description.text += "\n[TECH]";
        }
        else
            posture.text = string.Empty;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (playerActor == null) playerActor = BattleManager.instance.PlayerActors[0];
    }

}
