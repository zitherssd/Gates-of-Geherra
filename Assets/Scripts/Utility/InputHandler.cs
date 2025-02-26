using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

namespace Assets.Scripts.Utility
{
    [DefaultExecutionOrder(-1)]
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler instance = null;
        public InputActionReference touchPressed;
        public InputActionReference touchEnd;
        public InputActionReference touchHold;
        public InputActionReference touchPosition;
        public delegate void SwipeEvent(Vector2 delta);
        public delegate void ClickEvent(Vector2 clickPosition);
        public delegate void HoldEvent(Vector2 clickPosition);
        public event SwipeEvent OnSwipe;
        public event ClickEvent OnClick;
        public event HoldEvent OnHold;
        public OnScreenStick onScreenStick;

        private Vector2 startPos;
        private Vector2 endPos;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void OnEnable()
        {
            touchPressed.action.Enable();
            touchEnd.action.Enable();
            touchPosition.action.Enable();
            touchHold.action.Enable();
        }

        private void OnDisable()
        {
            touchPressed.action.Disable();
            touchEnd.action.Disable();
            touchPosition.action.Disable();
            touchHold.action.Disable();
        }

        private void Start()
        {
            touchPressed.action.performed += ctx => PressBegin(ctx);
            touchEnd.action.performed += ctx => PressEnd(ctx);
            touchHold.action.performed += ctx => PressHold(ctx);
            OnSwipe += delta => { Debug.Log("ONSWIPEEVENT TRIGGERED. DELTA IS " + delta); };
            OnClick += pos => { Debug.Log("ONCLICKEVENT TRIGGERED. POSITION IS " + pos); };
            OnHold += pos => { Debug.Log("ONHOLDEVENT TRIGGERED. POSITION IS " + pos); };
        }

        void PressBegin(InputAction.CallbackContext context)
        {
            startPos = touchPosition.action.ReadValue<Vector2>();
        }

        void PressEnd(InputAction.CallbackContext context)
        {
            endPos = touchPosition.action.ReadValue<Vector2>();
            if ((endPos - startPos).magnitude > 50f && OnSwipe != null) OnSwipe(endPos - startPos);
            else OnClick(endPos);
        }

        void PressHold(InputAction.CallbackContext context)
        {
            Debug.Log("HOLD PERFOREMD");
            var pos = touchPosition.action.ReadValue<Vector2>();
            OnHold(pos);
        }

        public void OnPointerUp()
        {
            //Vector2 stickPosition = onScreenStick.control.r;
            // Now you can use stickPosition
        }
    }
}
