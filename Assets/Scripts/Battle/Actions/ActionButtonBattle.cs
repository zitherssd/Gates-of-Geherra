using Assets.Scripts.Battle.Actions.Skills;
using Assets.Scripts.Battle.Actor;
using Assets.Scripts.Battle.Manager;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
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

        private Vector2 targetPos;

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

            if (JoystickManager.instance.IsPressed && (referencedAction.Type == BUTTONTYPE.CONTINUOUS_VECTOR || referencedAction.Type == BUTTONTYPE.VECTOR))
            {
                var deltaScaled = JoystickManager.instance.Direction;
                guide.transform.position = player.transform.position + GetRelativeToCamera(deltaScaled * referencedAction.StickMult);
                referencedAction.Direction = GetRelativeToCamera(deltaScaled);
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
                    JoystickManager.instance.EnableJoystick(Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition);
                    guide.GetComponent<ParticleSystem>().Play();
                    break;
                case BUTTONTYPE.CONTINNUOUS:
                    HideUIAndUseAction();
                    break;
                case BUTTONTYPE.CONTINUOUS_VECTOR:
                    referencedAction.Direction = Vector3.zero;
                    HideUIExceptThisAndUseAction();
                    JoystickManager.instance.EnableJoystick(Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition);
                    //guide.GetComponent<ParticleSystem>().Play();
                    referencedAction.OnCancel += () => JoystickManager.instance.DisableJoystick();
                    break;
                default:
                    break;
            }
        }

        public void OnPointerUp(PointerEventData pointerEventData)
        {
            if (button.IsInteractable() == false) return;
            if (Disabled) return;

            switch (referencedAction.Type)
            {
                case BUTTONTYPE.INSTANT:
                    HideUIAndUseAction();
                    break;
                case BUTTONTYPE.VECTOR:
                    HideUIAndUseAction();
                    JoystickManager.instance.DisableJoystick();
                    guide.GetComponent<ParticleSystem>().Stop();
                    break;
                case BUTTONTYPE.CONTINNUOUS:
                    referencedAction.Cancel();
                    JoystickManager.instance.DisableJoystick();
                    break;
                case BUTTONTYPE.CONTINUOUS_VECTOR:
                    referencedAction.Cancel();
                    UIManager.instance.GainMeter(referencedAction.SlowdownMeterGainOnRelease);
                    JoystickManager.instance.DisableJoystick();
                    guide.GetComponent<ParticleSystem>().Stop();
                    referencedAction.OnCancel -= () => JoystickManager.instance.DisableJoystick();
                    break;
                default:
                    break;
            }
        }

        public void HideUIAndUseAction()
        {
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
        private static GameObject guide;

        private RectTransform rect;
        private Vector3 homePosition;
    }
}
