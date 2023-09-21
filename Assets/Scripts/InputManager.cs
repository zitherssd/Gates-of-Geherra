using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager instance;
        [SerializeField] private InputActionReference StartPos;
        [SerializeField] private InputActionReference EndPos;
        [SerializeField] private InputActionReference DoubleTap;

        private BattleManager battleManager;
        private Action onTurnEnd = null;
        private Action onTargetSelected = null;
        private Action onSwipeEnded = null;
        private bool inputEnabled = false;

        [SerializeField] private bool waitingForTurn = false;
        [SerializeField] private bool waitingForTarget = false;
        [SerializeField] private bool waitingForSwipe = false;

        private BaseActorBattler selectedTarget;
        private Vector2 selectedSwipeDirection;

        public static InputManager GetInstance()
        {
            return instance;
        }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            battleManager = BattleManager.GetInstance();
        }

        private void Update()
        {
            if (!inputEnabled)
                return;

            var startpos = StartPos.action.ReadValue<Vector2>();
            var endpos = EndPos.action.ReadValue<Vector2>();
            var moveVector = endpos - startpos;

            if (waitingForTarget)
                CheckForTarget(startpos);

            if (waitingForTurn)
                CheckForTurn(moveVector);

            if (waitingForSwipe)
                CheckForSwipe(moveVector);
        }

        private void CheckForTarget(Vector2 startpos)
        {
            Ray raycast = Camera.main.ScreenPointToRay(startpos);
            if (Physics.Raycast(raycast, out RaycastHit raycastHit) && raycastHit.collider.name == "Enemy")
            {
                selectedTarget = raycastHit.collider.GetComponentInChildren<BaseActorBattler>();
                DisableInput();
                waitingForTarget = false;
                onTargetSelected?.Invoke();
            }
        }

        private void CheckForTurn(Vector2 moveVector)
        {
            if (moveVector.magnitude > 100)
            {
                DisableInput();
                waitingForTurn = false;
                var activeChar = battleManager.GetActiveActor();
                UIManager.GetInstance().CancelAll();

                var target = moveVector.normalized;
                var camera = Camera.main;
                var forward = camera.transform.forward; forward.y = 0;
                var right = camera.transform.right; right.y = 0;
                forward.Normalize(); right.Normalize();

                var desiredMoveDirection = forward * target.y + right * target.x;
                activeChar.Move(desiredMoveDirection, onTurnEnd);
            }

        }

        private void CheckForSwipe(Vector2 moveVector)
        {
            if (moveVector.magnitude > 100)
            {
                DisableInput();
                waitingForSwipe = false;
                selectedSwipeDirection = moveVector;
                onSwipeEnded?.Invoke();
            }
        }

        public void EnableInput()
        {
            inputEnabled = true;
        }

        public void DisableInput()
        {
            inputEnabled = false;
        }

        public void WaitForTurn(Action onTurnEnd)
        {
            this.onTurnEnd = onTurnEnd;
            waitingForTurn = true;
            EnableInput();
            UIManager.GetInstance().DrawActiveActorSkills(() =>
            {
                waitingForTurn = false;
                battleManager.GetActiveActor().UseSkill(UIManager.GetInstance().GetSelectedSkill(), onTurnEnd);
            });
        }

        public void WaitForSwipe(Action onSwipeEnded)
        {
            UIManager.GetInstance().ChangeStatus("SWIPE TO CHOOSE DIRECTION");
            EnableInput();
            this.onSwipeEnded = onSwipeEnded;
            waitingForSwipe = true;
        }

        public void WaitForTarget(Action onTargetSelected)
        {
            UIManager.GetInstance().ChangeStatus("SELECT TARGET");
            if (battleManager.GetActiveActor().isControllable())
            {
                EnableInput();
                this.onTargetSelected = onTargetSelected;
                waitingForTarget = true;
            }

            if (battleManager.EnemyActors.Count == 1)
            {
                selectedTarget = battleManager.EnemyActors[0];
                waitingForTarget = false;
                onTargetSelected?.Invoke();
            }
        }

        public BaseActorBattler GetSelectedTarget()
        {
            UIManager.GetInstance().ChangeStatus(string.Empty);
            var selTarget = selectedTarget;
            selectedTarget = null;
            return selTarget;
        }

        public Vector2 GetSwipeDirection()
        {
            return selectedSwipeDirection;
        }
    }
}