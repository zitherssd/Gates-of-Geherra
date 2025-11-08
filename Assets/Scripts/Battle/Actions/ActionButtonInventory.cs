using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.DefaultInputActions;

namespace Assets.Scripts.Battle.Actions
{
    public class ActionButtonInventory : MonoBehaviour
    {
        private static RectTransform ActionsInventory;
        private static GameObject actionHolder;
        private static GameObject skillHolder;
        private BaseAction referencedAction;
        private ActionButtonBattle ActionButtonBattle;

        public void Awake()
        {
            ActionButtonBattle = GetComponent<ActionButtonBattle>();
            if (ActionsInventory == null)
                ActionsInventory = UIManager.instance.SkillsInventoryViewport.GetComponent<RectTransform>();
            if (actionHolder == null) actionHolder = GameObject.Find("LeftContainer");
            if (skillHolder == null) skillHolder = GameObject.Find("RightContainer");
        }

        public void Start()
        {
            referencedAction = gameObject.GetComponent<ActionButtonHandler>().referencedAction;
        }

        public void MoveBettwenInventoryAndContainer()
        {
            if (gameObject.transform.parent == ActionsInventory)
            {
                if(referencedAction is AttackSkill)
                    gameObject.transform.SetParent(skillHolder.transform);
                else
                    gameObject.transform.SetParent(actionHolder.transform);
            }
            else
            {
                gameObject.transform.SetParent(ActionsInventory.transform);
            }
        }

        public void DisableBattleSkills()
        {

            ActionButtonBattle.enabled = false;
            this.enabled = true;
            var button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(MoveBettwenInventoryAndContainer);
        }
        public void EnableBattleSkills()
        {

            ActionButtonBattle.enabled = true;
            this.enabled = false;
            var button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
        }

    }
}
