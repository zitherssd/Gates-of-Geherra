using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Game;
using Assets.Scripts.Save;
using Assets.Scripts.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.InputSystem.DefaultInputActions;

namespace Assets.Scripts.UI
{
    public class ActionButtonInventory : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private static RectTransform ActionsInventory;
        private static GameObject actionHolder; // Left Main
        private static GameObject skillHolder; // Right Main
        private static GameObject leftSecondary;
        private static GameObject rightSecondary;

        private BaseAction referencedAction;
        private ActionButtonBattle ActionButtonBattle;
        
        private CanvasGroup canvasGroup;
        private Canvas canvas;
        [HideInInspector] public Transform originalParent;
        [HideInInspector] public int originalSiblingIndex;

        public void Awake()
        {
            ActionButtonBattle = GetComponent<ActionButtonBattle>();
            if (ActionsInventory == null)
                ActionsInventory = UIManager.instance.SkillsInventoryViewport.GetComponent<RectTransform>();
            
            // Use UIManager references
            if (actionHolder == null) actionHolder = UIManager.instance.ActionsHolder;
            if (skillHolder == null) skillHolder = UIManager.instance.SkillHolder;
            if (leftSecondary == null) leftSecondary = UIManager.instance.LeftSecondaryContainer;
            if (rightSecondary == null) rightSecondary = UIManager.instance.RightSecondaryContainer;
            
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Start()
        {
            referencedAction = gameObject.GetComponent<ActionButtonHandler>().referencedAction;
            canvas = GetComponentInParent<Canvas>();
        }

        public void DisableBattleSkills()
        {
            ActionButtonBattle.enabled = false;
            this.enabled = true;

            // Remove click listener as it's no longer used for moving items
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        public void EnableBattleSkills()
        {
            ActionButtonBattle.enabled = true;
            this.enabled = false;

            // Button listeners for battle are handled by ActionButtonBattle script
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!this.enabled) return;

            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();

            // Reparent to top-level canvas to render above all UI
            transform.SetParent(canvas.transform, true); 
            transform.SetAsLastSibling();

            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!this.enabled) return;
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!this.enabled) return;

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            // If it wasn't dropped on a valid slot, it will still be parented to the canvas.
            // In that case, return it to its original position.
            if (transform.parent == canvas.transform)
            {
                transform.SetParent(originalParent, false);
                transform.SetSiblingIndex(originalSiblingIndex);
            }
            else
            {
                // Successful drop: persist in-memory layout for the active scene context
                // (SandboxScene -> SandboxLoadout, other scenes -> regular Loadout).
                if (GameSession.Exists && UIManager.instance != null)
                    GameSession.Instance.SetActiveLoadout(UIManager.instance.GetCurrentLoadout());

                // Keep existing disk-save behavior for regular save-slot flow.
                if (SaveManager.instance != null)
                {
                    SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
                }
            }
        }
    }
}
