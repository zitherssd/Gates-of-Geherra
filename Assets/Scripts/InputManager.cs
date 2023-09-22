using System;
using System.Collections;
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
        [SerializeField] private InputActionReference Move;

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
        private Vector2 delta;

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
            Debug.Log(Move.action.phase);
            Debug.Log(Move.action.ReadValue<Vector2>());
            //if (waitingForTurn)
            //{
            //    if (Move.action.phase == InputActionPhase.Performed)
            //    {
            //        var delta = Move.action.ReadValue<Vector2>();
            //        battleManager.GetActiveActor().Move(delta, onTurnEnd);
            //        waitingForTurn = false;
            //        DisableInput();
            //    }
            //}

            //if (!inputEnabled)
            //    return;

            //if (waitingForTarget)
            //    CheckForTarget(StartPos.action.ReadValue<Vector2>());

            ////if (waitingForSwipe)
            ////    CheckForSwipe(move);
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

        private IEnumerator WaitForMove(Action onDeltaObtained)
        {
            Move.action.Enable();

            while (Move.action.phase != InputActionPhase.Started) yield return 0;

            this.delta = Move.action.ReadValue<Vector2>();
            Move.action.Disable();
            onDeltaObtained();
        }

        public void EnableInput()
        {
            Move.action.Enable();
            inputEnabled = true;
        }

        public void DisableInput()
        {
            inputEnabled = false;
        }

        public void WaitForTurn(Action onTurnEnd)
        {
            //Enable Inputs
            //Options: Move or Select Skill

            //when does the turn end? when you choose to call the callback;
            // when doe onTurnEnd run? i.e. Choosing next active character
            // after the movement has executed or the skill has finished acting
            UIManager.GetInstance().DrawActiveActorSkills(() => //one shot option for the skill
            {
                battleManager.GetActiveActor().UseSkill(UIManager.GetInstance().GetSelectedSkill(), onTurnEnd);
                waitingForTurn = false;
                DisableInput();
            });

            StartCoroutine(WaitForMove(() =>
            {
                if (waitingForTurn)
                {
                    battleManager.GetActiveActor().Move(delta, onTurnEnd);
                    waitingForTurn = false;
                    DisableInput();
                }
            }));


            this.onTurnEnd = onTurnEnd;
            waitingForTurn = true;
            EnableInput();
        }

        public IEnumerator GetSwipe(Action onSwipeGot)
        {
            EnableInput();

            while (true)
            {
                if (Move.action.phase == InputActionPhase.Performed)
                {
                    var delta = Move.action.ReadValue<Vector2>();
                    UIManager.GetInstance().CancelAll();
                    var camera = Camera.main;
                    var forward = camera.transform.forward; forward.y = 0;
                    var right = camera.transform.right; right.y = 0;
                    forward.Normalize(); right.Normalize();

                    var desiredMoveDirection = forward * delta.y + right * delta.x;
                    var activeChar = battleManager.GetActiveActor();
                    activeChar.Move(desiredMoveDirection, onTurnEnd);
                    onTurnEnd = null;
                }
                yield return 0;
            }

            DisableInput();
        }

        public void WaitForSwipe(Action onSwipeEnded)
        {
            UIManager.GetInstance().ChangeStatus("SWIPE TO CHOOSE DIRECTION");
            EnableInput();
            this.onSwipeEnded = onSwipeEnded;
            waitingForSwipe = true;
            //Start coroutine called GetSwipe(
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