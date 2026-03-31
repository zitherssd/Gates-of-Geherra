using UnityEngine;

namespace Assets.Scripts.UI
{
    public class JoystickManager : MonoBehaviour
    {
        public static JoystickManager instance;

        public RectTransform joystickBase;
        public RectTransform joystickKnob;
        public float maxJoystickDistance = 100f;

        public Vector2 Direction { get; private set; }
        public bool IsPressed { get; private set; }

        private Vector2 joystickCenter;

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void Start()
        {
            if (joystickBase == null) joystickBase = GameObject.Find("VirtualJoystickBase").GetComponent<RectTransform>();
            if (joystickKnob == null) joystickKnob = GameObject.Find("VirtualKnob").GetComponent<RectTransform>();
            DisableJoystick();
        }

        private void Update()
        {
            if (!joystickBase.gameObject.activeSelf) return;

            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                Vector2 currentPointerPosition;

                if (Input.touchCount > 0)
                {
                    currentPointerPosition = Input.GetTouch(0).position;
                }
                else
                {
                    currentPointerPosition = (Vector2)Input.mousePosition;
                }

                Vector2 deltax = currentPointerPosition - new Vector2(joystickBase.position.x, joystickBase.position.y);
                float magnitude = deltax.magnitude;
                if (magnitude > maxJoystickDistance)
                {
                    deltax = deltax.normalized * maxJoystickDistance;
                }
                joystickKnob.localPosition = deltax;

                Direction = deltax / 100;
                IsPressed = true;
            }
            else
            {
                IsPressed = false;
                Direction = Vector2.zero;
            }
        }

        public void EnableJoystick(Vector2 position)
        {
            joystickBase.gameObject.SetActive(true);
            joystickKnob.gameObject.SetActive(true);
            joystickBase.position = position;
        }

        public void DisableJoystick()
        {
            joystickBase.gameObject.SetActive(false);
            joystickKnob.gameObject.SetActive(false);
            IsPressed = false;
            Direction = Vector2.zero;
        }
    }
}
