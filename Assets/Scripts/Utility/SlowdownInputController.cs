using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Utility
{
    /// <summary>
    /// Handles pointer/touch hold detection while state slowdown is active.
    /// Delegates the actual timescale transition to SlowdownManager.
    /// </summary>
    public class SlowdownInputController : MonoBehaviour
    {
        [Tooltip("If true, input detection will ignore touches/clicks over UI elements.")]
        [SerializeField] private bool ignoreUiInteractions = true;
        [Tooltip("If true, any UI raycast blocks hold input. If false, only interactive UI (buttons/drags) blocks hold input.")]
        [SerializeField] private bool blockOnAnyUiRaycast = false;

        private bool holdActive;

        private void Update()
        {
            if (SlowdownManager.instance == null)
                return;

            if (!SlowdownManager.instance.IsStateSlowdownActive)
            {
                if (holdActive)
                    EndHold();

                return;
            }

            if (PointerDownDetected())
            {
                if (!holdActive)
                {
                    holdActive = true;
                    SlowdownManager.instance.StartHoldResume();
                }
            }
            else if (PointerUpDetected())
            {
                if (holdActive)
                {
                    EndHold();
                }
            }
        }

        private void EndHold()
        {
            holdActive = false;
            SlowdownManager.instance.EndHoldResume();
        }

        private bool PointerDownDetected()
        {
            if (Input.GetMouseButtonDown(0))
            {
                return !ignoreUiInteractions || !IsBlockingUiUnderPointer((Vector2)Input.mousePosition, -1);
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    Touch touch = Input.GetTouch(i);
                    return !ignoreUiInteractions || !IsBlockingUiUnderPointer(touch.position, touch.fingerId);
                }
            }

            return false;
        }

        private bool PointerUpDetected()
        {
            if (Input.GetMouseButtonUp(0))
                return true;

            for (int i = 0; i < Input.touchCount; i++)
            {
                var phase = Input.GetTouch(i).phase;
                if (phase == TouchPhase.Ended || phase == TouchPhase.Canceled)
                    return true;
            }

            return false;
        }

        private bool IsBlockingUiUnderPointer(Vector2 pointerPosition, int pointerId)
        {
            if (EventSystem.current == null)
                return false;

            // Fast path for teams that prefer the previous strict behavior.
            if (blockOnAnyUiRaycast)
            {
                if (pointerId >= 0)
                    return EventSystem.current.IsPointerOverGameObject(pointerId);

                return EventSystem.current.IsPointerOverGameObject();
            }

            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = pointerPosition,
                pointerId = pointerId
            };

            var raycastResults = ListPool<RaycastResult>.Get();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            bool blocked = false;
            for (int i = 0; i < raycastResults.Count; i++)
            {
                GameObject hitObject = raycastResults[i].gameObject;
                if (hitObject == null)
                    continue;

                if (IsInteractiveUi(hitObject))
                {
                    blocked = true;
                    break;
                }
            }

            ListPool<RaycastResult>.Release(raycastResults);
            return blocked;
        }

        private static bool IsInteractiveUi(GameObject uiObject)
        {
            // Selectable catches Button, Toggle, Slider, etc.
            if (uiObject.GetComponentInParent<Selectable>() != null)
                return true;

            // These handlers catch custom drag widgets that are not Selectable.
            if (ExecuteEvents.GetEventHandler<IBeginDragHandler>(uiObject) != null)
                return true;

            if (ExecuteEvents.GetEventHandler<IDragHandler>(uiObject) != null)
                return true;

            if (ExecuteEvents.GetEventHandler<IPointerDownHandler>(uiObject) != null)
                return true;

            return false;
        }

        // Lightweight list pool to avoid per-frame allocations during raycast checks.
        private static class ListPool<T>
        {
            private static readonly System.Collections.Generic.Stack<System.Collections.Generic.List<T>> Pool = new System.Collections.Generic.Stack<System.Collections.Generic.List<T>>();

            public static System.Collections.Generic.List<T> Get()
            {
                return Pool.Count > 0 ? Pool.Pop() : new System.Collections.Generic.List<T>(8);
            }

            public static void Release(System.Collections.Generic.List<T> list)
            {
                list.Clear();
                Pool.Push(list);
            }
        }
    }
}
