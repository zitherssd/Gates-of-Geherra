using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Game;
using Assets.Scripts.Save;
using Assets.Scripts.Utility;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private TextMeshProUGUI TopTextbox;
        [SerializeField] private TextMeshProUGUI MiddleTextbox;
        [SerializeField] private TextMeshProUGUI StoneSlab;
        [SerializeField] private UnityEngine.UI.Image fadeImage;
        [SerializeField] private CanvasGroup actionHolder;

        [Range(0, 1)] public float letterPause = 0.01f;
        [Range(0, 1)] public float fadeSpeed;
        public AudioClip typeSound1;
        public AudioClip typeSound2;

        [HideInInspector] public BaseAction selectedAction;
        [HideInInspector] public BaseSkill selectedSkill;

        public static UIManager instance;
        public GameObject OriginPoint;
        
        [Header("Action Containers")]
        public GameObject ActionsHolder; // Left Main
        public GameObject LeftSecondaryContainer;
        public GameObject SkillHolder; // Right Main
        public GameObject RightSecondaryContainer;
        
        public GameObject SkillsInventoryViewport;
        public GameObject RestingUI;
        public List<GameObject> PlayerActions = new List<GameObject>();
        private bool isLocked = false;

        public static event Action OnHideUI;
        public static event Action OnShowUI;

        public static UIManager GetInstance()
        {
            return instance;
        }

        #region Initialization

        public void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            // Cleanup if needed
        }

        public void Start()
        {
            OriginPoint = GameObject.FindGameObjectWithTag("OriginPoint");
        }

        void Update()
        {
            // Placeholder for other UI updates
        }

        #endregion

        #region Slowdown Management

        /// <summary>
        /// Deactivate state-based slowdown.
        /// Delegates to SlowdownManager which handles the fade back to normal time.
        /// </summary>
        public void ResetStateSlowdown()
        {
            if (SlowdownManager.instance != null)
            {
                SlowdownManager.instance.ResetStateSlowdown();
            }
        }

        internal void ResetMeter()
        {
            ResetStateSlowdown();
        }

        #endregion

        #region Action Button Management

        public void InitializePlayerActionButtonPrefabs(List <BaseAction> ActionsToInitialize, List<ActionSlotSaveData> loadout = null)
        {
            // If we are refreshing the UI and no loadout is provided, capture the current state
            // so we don't reset everything to default positions.
            if (loadout == null && PlayerActions.Count > 0)
            {
                loadout = GetCurrentLoadout();
            }

            var actions = new List<BaseAction>(ActionsToInitialize);
            //Destory exiting actions
            foreach (GameObject child in PlayerActions)
            {
                Destroy(child.gameObject);
            }
            PlayerActions.Clear();


            //Create new prefabs for exiting actions
            var handlers = new List<ActionButtonHandler>();
            
            // Keep track of which actions have been placed
            HashSet<BaseAction> placedActions = new HashSet<BaseAction>();

            // 1. Place actions based on loadout if available
            if (loadout != null && loadout.Count > 0)
            {
                foreach (var slotData in loadout)
                {
                    // Find the action in the player's list that matches the saved GUID
                    BaseAction action = actions.FirstOrDefault(a => a.guid == slotData.ActionGuid && !placedActions.Contains(a));
                    
                    if (action != null)
                    {
                        GameObject container = GetContainerByID(slotData.ContainerID);
                        if (container != null)
                        {
                            CreateAndPlaceButton(action, container, slotData.SlotIndex);
                            placedActions.Add(action);
                        }
                    }
                }
            }

            // 2. Place remaining actions (newly acquired or if no loadout exists)
            int defaultPlacedCount = 0;
            const int maxDefaultActions = 8;

            for(int i = 0; i < actions.Count; i++)
            {
                if (placedActions.Contains(actions[i])) continue;

                // Default placement logic if not in loadout
                if (loadout == null)
                {
                    if (defaultPlacedCount < maxDefaultActions)
                    {
                        // Legacy default placement
                        if (actions[i] is AttackSkill || actions[i] is ProjectileAttack)
                            CreateAndPlaceButton(actions[i], SkillHolder);
                        else
                            CreateAndPlaceButton(actions[i], ActionsHolder);
                        defaultPlacedCount++;
                    }
                    else
                    {
                        CreateAndPlaceButton(actions[i], SkillsInventoryViewport);
                    }
                }
                else
                {
                    // If we have a loadout but this item wasn't in it, put it in inventory
                    CreateAndPlaceButton(actions[i], SkillsInventoryViewport);
                }
            }
        }

        private void CreateAndPlaceButton(BaseAction action, GameObject parent, int siblingIndex = -1)
        {
            GameObject ActionButtonGameobject = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
            ActionButtonGameobject.GetComponent<ActionButtonHandler>().Init(action);
            PlayerActions.Add(ActionButtonGameobject);
            ActionButtonGameobject.transform.SetParent(parent.transform);
            if (siblingIndex >= 0)
            {
                ActionButtonGameobject.transform.SetSiblingIndex(siblingIndex);
            }
        }

        public GameObject GetContainerByID(string id)
        {
            switch (id)
            {
                case "Left": return ActionsHolder;
                case "LeftSec": return LeftSecondaryContainer;
                case "Right": return SkillHolder;
                case "RightSec": return RightSecondaryContainer;
                default: return SkillsInventoryViewport;
            }
        }

        public string GetIDByContainer(GameObject container)
        {
            if (container == ActionsHolder) return "Left";
            if (container == LeftSecondaryContainer) return "LeftSec";
            if (container == SkillHolder) return "Right";
            if (container == RightSecondaryContainer) return "RightSec";
            return "Inventory";
        }

        public List<ActionSlotSaveData> GetCurrentLoadout()
        {
            List<ActionSlotSaveData> loadout = new List<ActionSlotSaveData>();

            // Resolve the current player body. In a battle/arena it's BattleManager.Player; in the
            // rest scene that resolver falls back to GameFlowManager's player. This lets the loadout
            // be captured in arena scenes that have no GameFlowManager.
            var player = BattleManager.instance != null ? BattleManager.instance.Player : null;
            if (player == null && GameFlowManager.instance != null)
                player = GameFlowManager.instance.playerActor;

            if (player == null || player.Runtime == null)
            {
                // Cannot determine owned actions, return empty or log error.
                Debug.LogError("Could not get player actions to create loadout.");
                return loadout;
            }

            var allPlayerOwnedActions = player.Runtime.actions;

            // Create a lookup from action to its button GameObject for performance.
            var actionToButtonMap = PlayerActions
                .Where(go => go != null && go.GetComponent<ActionButtonHandler>() != null)
                .ToDictionary(go => go.GetComponent<ActionButtonHandler>().referencedAction, go => go);

            foreach (var action in allPlayerOwnedActions)
            {
                if (actionToButtonMap.TryGetValue(action, out GameObject buttonGO))
                {
                    // This action has a UI button, find its location.
                    Transform parent = buttonGO.transform.parent;
                    string containerID = GetIDByContainer(parent.gameObject);
                    int slotIndex = buttonGO.transform.GetSiblingIndex();

                    loadout.Add(new ActionSlotSaveData
                    {
                        ContainerID = containerID,
                        SlotIndex = slotIndex,
                        ActionGuid = action.guid
                    });
                }
                else
                {
                    // This action is owned but has no button. It should be in the inventory.
                    // This handles cases where an action was just awarded but the UI hasn't been refreshed.
                    loadout.Add(new ActionSlotSaveData
                    {
                        ContainerID = "Inventory", // Default to inventory
                        SlotIndex = -1, // No specific slot index
                        ActionGuid = action.guid
                    });
                }
            }
            return loadout;
        }

        public void SaveLoadout()
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.SaveToSlot(SaveManager.instance.currentSaveSlot);
            }
        }

        public void CreatePlayerActionButtonPrefab(BaseAction ActionToInitialize)
        {
            GameObject ActionButtonGameobject = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
            ActionButtonGameobject.GetComponent<ActionButtonHandler>().Init(ActionToInitialize);
            PlayerActions.Add(ActionButtonGameobject);
            ActionButtonGameobject.transform.SetParent(SkillsInventoryViewport.transform);
            ActionButtonGameobject.GetComponent<ActionButtonBattle>().enabled = false;
            ActionButtonGameobject.GetComponent<ActionButtonInventory>().enabled = true;
            ActionButtonGameobject.GetComponent<ActionButtonInventory>().DisableBattleSkills();
        }

        #endregion

        #region Text & Animation

        public IEnumerator TypeTextMiddleLetterByLetter(string text, Action onTypingComplete)
        {
            MiddleTextbox.text = string.Empty;
            MiddleTextbox.color = new Color(MiddleTextbox.color.r, MiddleTextbox.color.g, MiddleTextbox.color.b, 1);
            foreach (char letter in text.ToCharArray())
            {
                MiddleTextbox.text += letter;
                if (typeSound1 && typeSound2)
                    //SoundManager.instance.RandomizeSfx(typeSound1, typeSound2);
                yield return 0;
                yield return new WaitForSeconds(letterPause);
            }
            onTypingComplete();
        }

        public IEnumerator FadeMiddleText(float fadeDuration)
        {
            Color originalColor = MiddleTextbox.color;
            Color targetColor = new(originalColor.r, originalColor.g, originalColor.b, 0f); // Fade to transparent

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                MiddleTextbox.color = Color.Lerp(originalColor, targetColor, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            MiddleTextbox.color = targetColor;
            MiddleTextbox.text = string.Empty;
        }

        public void Fade(bool fadeIn, Action onFadeComplete)
        {
            if (!fadeIn)
            {
                LeanTween.alphaCanvas(actionHolder, 1f, 0.5f);
                LeanTween.alpha(fadeImage.rectTransform, 0f, 0.5f).setOnComplete(onFadeComplete);
            }
            else
            {
                LeanTween.alpha(fadeImage.rectTransform, 1f, 0.5f).setOnComplete(onFadeComplete);
            }
        }

        #endregion

        #region UI Visibility

        public void HideUI()
        {
            LeanTween.scale(ActionsHolder.gameObject, new Vector3(1, 0, 1), 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(SkillHolder.gameObject, new Vector3(1, 0, 1), 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            if (LeftSecondaryContainer != null)
                LeanTween.scale(LeftSecondaryContainer.gameObject, new Vector3(1, 0, 1), 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            if (RightSecondaryContainer != null)
                LeanTween.scale(RightSecondaryContainer.gameObject, new Vector3(1, 0, 1), 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            OnHideUI?.Invoke();
        }

        public void ShowUI()
        {
            if (isLocked) return;
            ActionsHolder.transform.parent.gameObject.SetActive(true);

            var leftContainerChildren = GetAllChildren(ActionsHolder);
            var rightContainerChildren = GetAllChildren(SkillHolder);
            var leftSecContainerChildren = LeftSecondaryContainer != null ? GetAllChildren(LeftSecondaryContainer) : new List<GameObject>();
            var rightSecContainerChildren = RightSecondaryContainer != null ? GetAllChildren(RightSecondaryContainer) : new List<GameObject>();

            foreach (var child in leftContainerChildren.Concat(rightContainerChildren).Concat(leftSecContainerChildren).Concat(rightSecContainerChildren))
            {
                child.GetComponent<ActionButtonBattle>().Disabled = false;
                LeanTween.scale(child.gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
            }
            LeanTween.scale(ActionsHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(SkillHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);

            if (LeftSecondaryContainer != null && LeftSecondaryContainer.transform.childCount > 0)
                LeanTween.scale(LeftSecondaryContainer.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);

            if (RightSecondaryContainer != null && RightSecondaryContainer.transform.childCount > 0)
                LeanTween.scale(RightSecondaryContainer.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);

            OnShowUI?.Invoke();
        }

        public void ShowUIIgnoreLocked()
        {
            ActionsHolder.transform.parent.gameObject.SetActive(true);

            var leftContainerChildren = GetAllChildren(ActionsHolder);
            var rightContainerChildren = GetAllChildren(SkillHolder);
            var leftSecContainerChildren = LeftSecondaryContainer != null ? GetAllChildren(LeftSecondaryContainer) : new List<GameObject>();
            var rightSecContainerChildren = RightSecondaryContainer != null ? GetAllChildren(RightSecondaryContainer) : new List<GameObject>();

            foreach (var child in leftContainerChildren.Concat(rightContainerChildren).Concat(leftSecContainerChildren).Concat(rightSecContainerChildren))
            {
                child.GetComponent<ActionButtonBattle>().Disabled = false;
                LeanTween.scale(child.gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
            }
            LeanTween.scale(ActionsHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(SkillHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(LeftSecondaryContainer.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(RightSecondaryContainer.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);

            OnShowUI?.Invoke();
        }

        public void DisableUI()
        {
            HideUI();
            isLocked = true;
        }

        public void EnableUI()
        {
            ShowUI();
            isLocked = false;
        }

        internal void ShowRestingUI()
        {
            RestingUI.SetActive(true);
        }

        #endregion

        #region Battle Skills Management

        public void DisableBattleSkills()
        {
            foreach (var actionGO in PlayerActions)
            {
                if (actionGO == null) continue;
                
                var inventoryScript = actionGO.GetComponent<ActionButtonInventory>();
                if (inventoryScript != null)
                {
                    inventoryScript.DisableBattleSkills();
                }
            }
        }

        public void EnableBattleSkills()
        {
            foreach (var actionGO in PlayerActions)
            {
                if (actionGO == null) continue;

                var inventoryScript = actionGO.GetComponent<ActionButtonInventory>();
                if (inventoryScript != null)
                {
                    inventoryScript.EnableBattleSkills();
                }
            }
        }

        #endregion

        #region Utility Methods

        public void HideAllButThis(BaseAction action)
        {
            var leftContainerChildren = GetAllChildren(ActionsHolder);
            var rightContainer = GetAllChildren(SkillHolder);
            HideUI();
            HideIfNotMatch(leftContainerChildren.Concat(rightContainer).ToList(), action);
        }

        public void HideIfNotMatch(List<GameObject> children, BaseAction action)
        {
            foreach (GameObject child in children)
            {
                ActionButtonBattle buttonHandler = child.GetComponent<ActionButtonBattle>();
                if (buttonHandler != null && buttonHandler.referencedAction == action)
                {
                    // Keep this button visible
                }
                else
                {
                    buttonHandler.Disabled = true;
                    LeanTween.scale(buttonHandler.gameObject, new Vector3(1, 0, 1), 0.1f).setEaseInOutCubic();
                }
            }
        }

        public List<GameObject> GetAllChildren(GameObject parent)
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject);
            }
            return children;
        }

        #endregion
    }
}
