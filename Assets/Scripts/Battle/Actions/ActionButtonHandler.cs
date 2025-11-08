using System;
using System.Collections.Generic;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Utility
{
    public class ActionButtonHandler : MonoBehaviour
    {

        public Image image;
        public Image cooldownimage;
        private Transform home;
        private Button button;
        public BaseAction referencedAction;

        public void Init(BaseAction action)
        {
            GetComponent<ActionButtonBattle>().referencedAction = action;
            referencedAction = action;
            string plusSymbol = "+";
            if (referencedAction.TotalUses != 0 && referencedAction.remainingUses > 0)
            {
                remainingUses.text = ConcatWithPlus(plusSymbol, referencedAction.remainingUses);
            }
            else
            {
                remainingUses.text = string.Empty;
            }
            if (referencedAction)
                cooldownimage.fillAmount = Mathf.Clamp(referencedAction.currentCooldownTimer / referencedAction.CooldownTimer, 0, 1);
            SetUIFromAction(action);
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

        private void SetUIFromAction(BaseAction action)
        {
            var sr = GetComponent<Image>();
            if (action != null)
            {
                switch (action.Type)
                {
                    case BUTTONTYPE.INSTANT:
                        sr.sprite = Resources.Load<Sprite>("Sprites/ButtonBorderInstant");
                        break;
                    case BUTTONTYPE.VECTOR:
                        sr.sprite = Resources.Load<Sprite>("Sprites/ButtonBorderVector");
                        break;
                    case BUTTONTYPE.CONTINNUOUS:
                        sr.sprite = Resources.Load<Sprite>("Sprites/ButtonBorderContinuous");
                        break;
                    case BUTTONTYPE.CONTINUOUS_VECTOR:
                        sr.sprite = Resources.Load<Sprite>("Sprites/ButtonBorderVector");
                        break;
                }

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


            }
        }

        private string ConcatWithPlus(string symbol, int value)
        {
            return new string(symbol[0], value);
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

    }
}
