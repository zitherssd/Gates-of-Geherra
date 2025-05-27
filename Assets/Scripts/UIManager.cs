using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Utility;
using TMPro;
using UnityEngine;

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

        [HideInInspector] public BaseReaction selectedReaction;
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
        public GameObject RestingUI;
        private bool effectActive;
        [SerializeField] private AnimationCurve startCurve;
        [SerializeField] private AnimationCurve startCurve2;
        [SerializeField] private AnimationCurve endCurve;
        private float totalEffectTime;
        private bool isLocked = false;

        public Action<BaseAction> OnActionSelected { get; private set; }
        public Action<BaseReaction> OnReactionSelected { get; private set; }

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
            totalEffectTime = seconds + SlowdownMeter.value * 5;
            SlowdownMeter.gameObject.SetActive(true);
            SlowdownMeter.value += seconds / 5;

            Debug.Log($"GainMeter called: SlowdownMeter.value is now {SlowdownMeter.value}");
            // Recalculate the total effect time based on the new SlowdownMeter value

            // If the effect is active, stop the current coroutine and restart it
            if (effectActive)
            {
                StopAllCoroutines();  // Stop the currently running effect
            }

            // Start the effect with the updated duration
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

        public void ChangeStatus(string status)
        {
            MiddleTextbox.text = status;
        }

        public void AddToStoneSlab(string textToAdd)
        {
            var previous = StoneSlab.text;
            StoneSlab.text = textToAdd + Environment.NewLine;
            StoneSlab.text += previous;
        }

        public List<ActionButtonHandler> DrawActionsRadiallyOnScreenPoint(List<BaseAction> actions)
        {
            foreach(Transform child in ActionsHolder.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in SkillHolder.transform) { Destroy(child.gameObject); }
            LeanTween.scale(ActionsHolder, Vector3.one, 0.2f).setEaseOutCubic().setIgnoreTimeScale(true);
            float radius = 100f;
            var handlers = new List<ActionButtonHandler>();

            float anglestep = 360f / actions.Count;
            for (int i = 0; i < actions.Count; i++)
            {
                float angle = i * anglestep;
                float x = radius * Mathf.Cos(Mathf.Deg2Rad * angle);
                float y = radius * Mathf.Sin(Mathf.Deg2Rad * angle);
                Vector2 position = new(x, y);

                GameObject UISkill = Instantiate(buttonPrefab, position, Quaternion.identity);
                if (actions[i] is AttackSkill or ProjectileAttack)
                    UISkill.transform.SetParent(SkillHolder.transform);
                else
                    UISkill.transform.SetParent(ActionsHolder.transform);

                UISkill.GetComponent<RectTransform>().localPosition = position;
                var handler = UISkill.GetComponent<ActionButtonHandler>();
                handler.Initialize(actions[i]);
                handlers.Add(handler);
            }
            //EndButton.gameObject.SetActive(true);
            //ActionSlot.SetActive(true);
            return handlers;
        }




        public void DrawActions(List<BaseAction> ActionsToDraw)
        {
            var buttonHandlers = DrawActionsRadiallyOnScreenPoint(ActionsToDraw);

            for (int i = 0; i < ActionsToDraw.Count; i++)
            {
                buttonHandlers[i].referencedAction = ActionsToDraw[i];
            }
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
                LeanTween.alphaCanvas(actionHolder, 0f, 0.5f);
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
                ActionButtonHandler buttonHandler = child.GetComponent<ActionButtonHandler>();

                // Check if the child has a ButtonHandler and if its referencedSkill matches the action
                if (buttonHandler != null && buttonHandler.referencedAction == action)
                {
                    Debug.Log("Found child with matching referencedSkill: " + child.name);
                }
                else
                {
                    buttonHandler.Disabled = true;
                    LeanTween.scale(buttonHandler.gameObject, new Vector3(1,0,1), 0.1f).setEaseInOutCubic();
                }
                
            }

            Debug.Log("No child with the matching referencedSkill found.");
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
        }
        public void ShowUI()
        {
            if (isLocked) return;
            ActionsHolder.transform.parent.gameObject.SetActive(true);

            var leftContainerChildren = GetAllChildren(ActionsHolder);
            var rightContainer = GetAllChildren(SkillHolder);
        
            foreach(var child in leftContainerChildren.Concat(rightContainer))
            {
                child.GetComponent<ActionButtonHandler>().Disabled = false;
                LeanTween.scale(child.gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
            }
            LeanTween.scale(ActionsHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(SkillHolder.gameObject, Vector3.one, 0.15f).setEaseInOutCubic().setIgnoreTimeScale(true);
        }
        internal void ResetMeter()
        {
            SlowdownMeter.value = 0f;
            Debug.Log("ResetMeter called: SlowdownMeter.value set to 0");
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

        //1. Attack or Move or Skill // MoveWithingRange if able;
        //2. Reaction Check > Give control to the enemy. Allow him to chose from his reactions (Skill used for mitigation damage)
        //3. Deal Dmg, Apply Effects, Check for Posture break
        ////If Posture break > go back to 1.
        //4. ChoseNextActivePlayer();
        //
    }
}
