using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.InputSystem.Layouts;
using System;
using UnityEngine.Events;

namespace UnityEngine.InputSystem.OnScreen
{
    public class ModifiedOnScreenStick : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out m_PointerDownPos);
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
                throw new System.ArgumentNullException(nameof(eventData));

            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
            var delta = position - m_PointerDownPos;

            delta = Vector2.ClampMagnitude(delta, movementRange);
            ((RectTransform)transform).anchoredPosition = m_StartPos + (Vector3)delta;

            var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
        }
        public new void OnEnable()
        {
            ((RectTransform)transform).anchoredPosition = m_StartPos;
            OnValueChanged.Invoke(GetStickValue());
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if(OnValueChanged != null)
            {
                OnValueChanged.Invoke(GetStickValue());
            }
        }
        private Vector2 GetStickValue()
        {
            return new Vector2(transform.localPosition.x / 50, transform.localPosition.y / 50);
        }
        private void Start()
        {
            m_StartPos = ((RectTransform)transform).anchoredPosition;
        }


        public UnityEvent<Vector2> OnValueChanged;
        public float movementRange
        {
            get => m_MovementRange;
            set => m_MovementRange = value;
        }

        [FormerlySerializedAs("movementRange")]
        [SerializeField]
        private float m_MovementRange = 50;

        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string m_ControlPath;

        private Vector3 m_StartPos;
        private Vector2 m_PointerDownPos;

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }
    }
}
