using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
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
        [SerializeField] private UnityEngine.UI.Slider SlowdownMeter;

        [Range(0, 1)] public float letterPause = 0.01f;
        [Range(0, 1)] public float fadeSpeed;
        public AudioClip typeSound1;
        public AudioClip typeSound2;

        [HideInInspector] public BaseAction selectedAction;
        [HideInInspector] public BaseSkill selectedSkill;

        public static UIManager instance;
        private float fadeduration = 1;
        private float timer = 0f;
        private bool waitingForAction = false;
        private Action onActionSelected;
        public GameObject OriginPoint;
        public GameObject ActionsHolder;
        public GameObject SkillHolder;
        public GameObject SkillsInventoryViewport;
        public GameObject RestingUI;
        public List<GameObject> PlayerActions = new List<GameObject>();
        private bool effectActive;
        [SerializeField] private AnimationCurve startCurve;
        [SerializeField] private AnimationCurve startCurve2;
        [SerializeField] private AnimationCurve endCurve;
        private float totalEffectTime;
        private bool isLocked = false;

        public static event Action OnHideUI;
        public static event Action OnShowUI;

        public static UIManager GetInstance()
        {
            return instance;
        }
        public void Awake()
        {
            instance = this;
            effectActive = true;
        }

        public void Start()
        {
            OriginPoint = GameObject.FindGameObjectWithTag("OriginPoint");
        }

        void Update()
        {
            if (SlowdownMeter.value > 0)
            {
                if (Time.unscaledDeltaTime > 0.1) return;
                SlowdownMeter.value -= 0.1f * Time.unscaledDeltaTime; // Decrease slider value over time
                if (!effectActive)
                {
                    StartCoroutine(StartEffect()); // Trigger effect when value is above 0 and effect is not active
                }

                SlowdownMeter.gameObject.SetActive(true);
            }
            else
            {
                if (effectActive)
                {
                    StartCoroutine(StopEffect(0.1f)); // Stop the effect when slider reaches 0
                }
                if (SlowdownMeter.gameObject == true)
                {
                    SlowdownMeter.value = 0; // Ensure the value doesn't go below 0
                    SlowdownMeter.gameObject.SetActive(false);
                }
            }
        }

        // Method to start the effect

        public IEnumerator StartEffect()
        {
            effectActive = true;
            float elapsedTime = 0f;

            while (elapsedTime < totalEffectTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                Time.timeScale = startCurve.Evaluate(elapsedTime / totalEffectTime);
                Time.fixedDeltaTime = Time.timeScale * .02f;

                yield return null;
            }
        }

        public IEnumerator StarEffectEnd()
        {
            effectActive = true;
            float elapsedTime = 0f;

            while (elapsedTime < totalEffectTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                Time.timeScale = startCurve2.Evaluate(elapsedTime / totalEffectTime);
                Time.fixedDeltaTime = Time.timeScale * .02f;

                yield return null;
            }
        }

        // Method to stop the effect
        public IEnumerator StopEffect(float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                Time.timeScale = endCurve.Evaluate(elapsedTime / duration);
                Time.fixedDeltaTime = Time.timeScale * .02f;

                yield return null;
            }

            Time.timeScale = 1f;  // Ensure time scale is back to normal (1)
            Time.fixedDeltaTime = Time.timeScale * .02f;
            effectActive = false; // Mark the effect as inactive
        }
       
        public void GainMeter(float seconds)
        {
            if (isLocked) return;
            totalEffectTime = seconds + SlowdownMeter.value * 5;
            SlowdownMeter.gameObject.SetActive(true);
            SlowdownMeter.value += seconds / 5;


            if (effectActive)
            {
                StopAllCoroutines();  
            }

            StartCoroutine(StartEffect());
        }

        public void GainMeterEndBattle(float seconds)
        {
            totalEffectTime = 1.5f;
            SlowdownMeter.gameObject.SetActive(false);
            //SlowdownMeter.value += seconds / 5;

           
            if (effectActive)
            {
                StopAllCoroutines();  // Stop the currently running effect
            }

            // Start the effect with the updated duration
            StartCoroutine(StarEffectEnd());
        }

        public void InitializePlayerActionButtonPrefabs(List <BaseAction> ActionsToInitialize)
        {
            var actions = new List<BaseAction>(ActionsToInitialize);
            //Destory exiting actions
            foreach (GameObject child in PlayerActions)
            {
                Destroy(child.gameObject);
            }
            PlayerActions.Clear();


            //Create new prefabs for exiting actions
            var handlers = new List<ActionButtonHandler>();
            for(int i = 0; i < actions.Count; i++)
            {
                GameObject ActionButtonGameobject = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity);
                ActionButtonGameobject.GetComponent<ActionButtonHandler>().Init(ActionsToInitialize[i]);
                PlayerActions.Add(ActionButtonGameobject);
                if (ActionsToInitialize[i] is AttackSkill || ActionsToInitialize[i] is ProjectileAttack)
                    ActionButtonGameobject.transform.SetParent(SkillHolder.transform);
                else
                    ActionButtonGameobject.transform.SetParent(ActionsHolder.transform);
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

            // Ensure the target color is reached
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

        public void HideAllButThis(BaseAction action)
        {
            var leftContainerChildren = GetAllChildren(ActionsHolder);
            var rightContainer = GetAllChildren(SkillHolder);
            HideUI();
            HideIfNotMatch(leftContainerChildren.Concat(rightContainer).ToList(), action);
        }
        public void HideIfNotMatch(List<GameObject> children, BaseAction action)
        {
            // Loop through the children to find the one with the correct ButtonHandler
            foreach (GameObject child in children)
            {
                ActionButtonBattle buttonHandler = child.GetComponent<ActionButtonBattle>();

                // Check if the child has a ButtonHandler and if its referencedSkill matches the action
                if (buttonHandler != null && buttonHandler.referencedAction == action)
                {
                }
                else
                {
                    buttonHandler.Disabled = true;
                    LeanTween.scale(buttonHandler.gameObject, new Vector3(1,0,1), 0.1f).setEaseInOutCubic();
                }
                
            }
        }
        public List<GameObject> GetAllChildren(GameObject parent)
        {
            List<GameObject> children = new List<GameObject>();

            // Loop through each child of the parent
            foreach (Transform child in parent.transform)
            {
                children.Add(child.gameObject); // Add child to list
            }

            return children;
        }
        public void HideUI()
        {
            LeanTween.scale(ActionsHolder.gameObject, new Vector3(1, 0, 1), 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(SkillHolder.gameObject, new Vector3(1, 0, 1), 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            //ActionsHolder.transform.parent.gameObject.SetActive(false);
            OnHideUI?.Invoke();
        }
        public void ShowUI()
        {
            if (isLocked) return;
            ActionsHolder.transform.parent.gameObject.SetActive(true);

            var leftContainerChildren = GetAllChildren(ActionsHolder);
            var rightContainer = GetAllChildren(SkillHolder);
        
            foreach(var child in leftContainerChildren.Concat(rightContainer))
            {
                child.GetComponent<ActionButtonBattle>().Disabled = false;
                LeanTween.scale(child.gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
            }
            LeanTween.scale(ActionsHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(SkillHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            OnShowUI?.Invoke();
        }

        public void ShowUIIgnoreLocked()
        {
            ActionsHolder.transform.parent.gameObject.SetActive(true);

            var leftContainerChildren = GetAllChildren(ActionsHolder);
            var rightContainer = GetAllChildren(SkillHolder);

            foreach (var child in leftContainerChildren.Concat(rightContainer))
            {
                child.GetComponent<ActionButtonBattle>().Disabled = false;
                LeanTween.scale(child.gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
            }
            LeanTween.scale(ActionsHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(SkillHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            OnShowUI?.Invoke();
        }

        internal void ResetMeter()
        {
            SlowdownMeter.value = 0f;
            StopAllCoroutines();
            StartCoroutine(StopEffect(0.05f));
        }

        internal void ShowRestingUI()
        {
            RestingUI.SetActive(true);
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

        public void DisableBattleSkills()
        {
            foreach (var action in PlayerActions)
            {
                action.GetComponent<ActionButtonInventory>().DisableBattleSkills();
            }
        }

        public void EnableBattleSkills()
        {
            foreach (var action in PlayerActions)
            {
                action.GetComponent<ActionButtonInventory>().EnableBattleSkills();
            }
        }

        //1. Attack or Move or Skill // MoveWithingRange if able;
        //2. Reaction Check > Give control to the enemy. Allow him to chose from his reactions (Skill used for mitigation damage)
        //3. Deal Dmg, Apply Effects, Check for Posture break
        ////If Posture break > go back to 1.
        //4. ChoseNextActivePlayer();
        //
    }
}
