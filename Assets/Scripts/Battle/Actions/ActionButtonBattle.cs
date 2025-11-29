using System;
using System.Collections.Generic;
using Assets.Scripts.Battle;
using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.Scripts.Battle.Actions.BaseAction;

namespace Assets.Scripts.Battle.Actions
{
    public class ActionButtonBattle : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public BaseAction referencedAction;

        public Image cooldownimage;
        private Transform home;
        private Button button;
        public bool Disabled = false;

        private static float maxJoystickDistance = 100f;
        private static RectTransform joystickBase;
        private static RectTransform joystickKnob;

        private Vector2 targetPos;
        private static Vector2 joystickCenter;

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }

        private void Awake()
        {
            button = gameObject.GetComponent<Button>();
            rect = gameObject.GetComponent<RectTransform>();

            if (actionHolder == null) actionHolder = GameObject.Find("LeftContainer");
            if (skillHolder == null) skillHolder = GameObject.Find("RightContainer");
            if (guide == null) guide = GameObject.Find("PlacementGuide");
            if (joystickBase == null) joystickBase = GameObject.Find("VirtualJoystickBase").GetComponent<RectTransform>();
            if (joystickKnob == null) joystickKnob = GameObject.Find("VirtualJoystickKnob").GetComponent<RectTransform>();

        }

        private void Start()
        {
            if (referencedAction == null) referencedAction = GetComponent<ActionButtonHandler>().referencedAction;
            if (player == null) player = BattleManager.instance.PlayerActors[0];
            home = transform.parent;
            LeanTween.scale(gameObject, Vector3.one, 0.4f).setEaseOutBack().setIgnoreTimeScale(true);
        }

        private void Update()
        {

            SetInteractable(referencedAction.IsValid(player, out _));

            if (referencedAction)
                cooldownimage.fillAmount = Mathf.Clamp(referencedAction.currentCooldownTimer / referencedAction.CooldownTimer, 0, 1);

            if (Disabled) return;
            if (!isPressed) return;
            // Check if the pointer is currently pressed down
            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                Vector2 currentPointerPosition;

                // Check if there is touch input
                if (Input.touchCount > 0)
                {
                    // Use the position of the first touch
                    currentPointerPosition = Input.GetTouch(0).position;
                }
                else
                {
                    // Use the position of the mouse pointer
                    currentPointerPosition = (Vector2)Input.mousePosition;
                }

                if (referencedAction.Type == BUTTONTYPE.CONTINUOUS_VECTOR || referencedAction.Type == BUTTONTYPE.VECTOR)
                {
                    Vector2 deltax = currentPointerPosition - new Vector2(joystickBase.position.x, joystickBase.position.y);
                    float magnitude = deltax.magnitude;
                    if (magnitude > maxJoystickDistance)
                    {
                        deltax = deltax.normalized * maxJoystickDistance;
                        if (magnitude > 3 * maxJoystickDistance)
                            joystickBase.position = Vector2.MoveTowards(joystickBase.position, (Vector2)joystickBase.position + deltax, 20 * Time.deltaTime);
                    }
                    joystickKnob.localPosition = deltax;
                
                    deltaScaled = deltax / 100;
                    guide.transform.position = player.transform.position + GetRelativeToCamera(deltaScaled * referencedAction.StickMult);
                    referencedAction.Direction = GetRelativeToCamera(deltaScaled);
                }
            }
        }

        private void OnDisable()
        {
            cooldownimage.fillAmount = 0;
            button.interactable = true;
        }
        private void MoveToCenter()
        {
            // Calculate the center position of the screen
            Vector2 centerPosition = new(Screen.width / 2f, Screen.height / 2f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            targetPos = CanvasHandler.instance.GetComponent<RectTransform>().anchoredPosition;
            //LeanTween.move(rect, CanvasHandler.instance.GetComponent<RectTransform>().anchoredPosition, 0.15f).setEaseOutBack().setIgnoreTimeScale(true);
            LeanTween.scale(rect, Vector3.one * 1.5f, 1f).setEaseOutBack().setIgnoreTimeScale(true);
            if (referencedAction.Tags.Contains(TAG.USESTICK))
                guide.GetComponent<ParticleSystem>().Play();
        }

        private void MoveToHome()
        {
            isPressed = false;
            LeanTween.cancel(rect);
            LeanTween.move(rect, homePosition, 0.15f).setEaseOutCubic().setIgnoreTimeScale(true);
            LeanTween.scale(rect, Vector3.one, 0.5f).setEaseOutCubic().setIgnoreTimeScale(true);
            guide.GetComponent<ParticleSystem>().Stop();

        }

        public void OnPointerDown(PointerEventData pointerEventData)
        {
            if (Disabled)
            {
             
                return;
            }

            if (button.IsInteractable() == false) return;

            if (referencedAction is AttackSkill)
            {
                CameraManager.instance.SlowTrack = false;
            }

            homePosition = gameObject.transform.position;

            switch (referencedAction.Type)
            {
                case BUTTONTYPE.INSTANT:
                    //UseSkill nothing else
                    break;
                case BUTTONTYPE.VECTOR:
                    CameraManager.instance.SlowTrack = false;
                    UIManager.instance.HideAllButThis(referencedAction);
                    UIManager.instance.GainMeter(referencedAction.SlowDownMeterGainOnPress);
                    EnableJoystick(true);
                    guide.GetComponent<ParticleSystem>().Play();

                    isPressed = true;
                    pointerDownPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                    joystickBase.position = pointerDownPosition;
                    break;
                case BUTTONTYPE.CONTINNUOUS:
                    HideUIAndUseAction();
                    break;
                case BUTTONTYPE.CONTINUOUS_VECTOR:
                    deltaScaled = Vector2.zero;
                    referencedAction.Direction = Vector3.zero;
                    HideUIExceptThisAndUseAction();
                    isPressed = true;
                    pointerDownPosition = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                    EnableJoystick(true);
                    //guide.GetComponent<ParticleSystem>().Play();
                    referencedAction.OnCancel += DisableJoystick; 

                    joystickBase.position = pointerDownPosition;
                    break;
                default:
                    break;
            }

            //Output the name of the GameObject that is being clicked
        }

        public static void DisableJoystick()
        {
            joystickBase.gameObject.SetActive(false);
            joystickKnob.gameObject.SetActive(false);
        }
        public static void EnableJoystick(bool active)
        {
            joystickBase.gameObject.SetActive(active);
            joystickKnob.gameObject.SetActive(active);
        }

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            if (button.IsInteractable() == false) return;
            isPressed = false;
            if (Disabled) return;

            switch (referencedAction.Type)
            {
                case BUTTONTYPE.INSTANT:
                    HideUIAndUseAction();
                    break;
                case BUTTONTYPE.VECTOR:
                    HideUIAndUseAction();
                    EnableJoystick(false);
                    guide.GetComponent<ParticleSystem>().Stop();
                    break;
                case BUTTONTYPE.CONTINNUOUS:
                    referencedAction.OnCancel?.Invoke();
                    EnableJoystick(false);
                    break;
                case BUTTONTYPE.CONTINUOUS_VECTOR:
                    referencedAction.OnCancel?.Invoke();
                    UIManager.instance.GainMeter(referencedAction.SlowdownMeterGainOnRelease);
                    EnableJoystick(false);
                    guide.GetComponent<ParticleSystem>().Stop();
                    referencedAction.OnCancel -= DisableJoystick;



                    break;
                default:
                    break;
            }
            isPressed = false;
        }

        public void HideUIAndUseAction()
        {
            isPressed = false;
            UIManager.instance.HideUI();
            player.UseAction(referencedAction, () => { player.state.TransitionToIdle(); });
        }
        public void HideUIExceptThisAndUseAction()
        {
            UIManager.instance.HideAllButThis(referencedAction);
            player.UseAction(referencedAction, () => { player.state.TransitionToIdle(); });
        }

        public Vector3 GetRelativeToCamera(Vector2 direction)
        {
            var camera = Camera.main;
            var forward = camera.transform.forward; forward.y = 0;
            var right = camera.transform.right; right.y = 0;
            forward.Normalize(); right.Normalize();

            var desiredMoveDirection = forward * direction.y + right * direction.x;
            return desiredMoveDirection;
        }

        private Actor.Actor player;
        private static GameObject actionHolder;
        private static GameObject skillHolder;
        private static GameObject actionSlot;
        private static ActionSlot actionSlotScript;
        private static GameObject guide;

        private RectTransform rect;
        private bool isPressed;
        private Vector2 pointerDownPosition;
        private Vector3 homePosition;
        private Vector2 deltaScaled;
    }
}
