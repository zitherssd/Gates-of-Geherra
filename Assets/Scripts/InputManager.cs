using Assets.Scripts.Actions;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager instance;
        [SerializeField]
        private InputActionReference StartPos;
        [SerializeField]
        private InputActionReference EndPos;
        private BattleManager battleManager;
        private Action onTurnEnd = null;
        private Action onTargetSelected = null;
        private Action onSwipeEnded = null;
        private bool inputEnabled = false;
        [SerializeField]
        private bool waitingForTurn = false;
        [SerializeField]
        private bool waitingForTarget = false;
        [SerializeField]
        private bool waitingForSwipe = false;
        private BaseActorBattler selectedTarget;
        private Vector2 selectedSwipeDirection;


        public static InputManager GetInstance()
        {
            return instance;
        }

        internal void WaitForSwipe(Action onSwipeEnded)
        {
            UIManager.GetInstance().ChangeStatus("SWIPE TO CHOOSE DIRECTION");
            this.onSwipeEnded = onSwipeEnded;
            this.waitingForSwipe = true;
        }

        internal Vector2 GetSwipeDirection()
        {
            UIManager.GetInstance().ChangeStatus(string.Empty);
            return selectedSwipeDirection;
        }

        private void Awake()
        {
            instance = this;
        }


        // Use this for initialization
        void Start()
        {
            battleManager = BattleManager.GetInstance();
        }

        // Update is called once per frame
        void Update()
        {
            var startpos = StartPos.action.ReadValue<Vector2>();
            var endpos = EndPos.action.ReadValue<Vector2>();
            var moveVector = endpos - startpos;
            Ray raycast = Camera.main.ScreenPointToRay(startpos);
            RaycastHit raycastHit;

            if (Physics.Raycast(raycast, out raycastHit))
            {
                if (raycastHit.collider.name == "Enemy")
                {
                    if (waitingForTarget)
                    {
                        selectedTarget = raycastHit.collider.GetComponentInChildren<BaseActorBattler>();
                        DisableInput();
                        waitingForTarget = false;
                        onTargetSelected();
                    }
                }
            }

            if (moveVector.magnitude > 100 && waitingForTurn) //Move
            {
                DisableInput();
                waitingForTurn = false;
                var activeChar = battleManager.GetActiveActor();
                UIManager.GetInstance().CancelAll();
                activeChar.MoveToPosition(moveVector, onTurnEnd);

            }
            if (moveVector.magnitude > 100 && waitingForSwipe)
            {
                DisableInput();
                waitingForSwipe = false;
                selectedSwipeDirection = moveVector;
                onSwipeEnded();
            }
        }

        public void EnableInput()
        {
            this.inputEnabled = true;
        }

        public void DisableInput()
        {
            this.inputEnabled = false;
        }


        public void WaitForTurn(Action onTurnEnd)
        {
            this.onTurnEnd = onTurnEnd;
            this.waitingForTurn = true;
            EnableInput();
            UIManager.GetInstance().DrawActiveActorSkills(() =>
            {
                waitingForTurn = false;
                battleManager.GetActiveActor().UseSkill(UIManager.GetInstance().GetSelectedSkill(), onTurnEnd);
            });

        }

        public void WaitForTarget(Action onTargetSelected)
        {
            UIManager.GetInstance().ChangeStatus("SELECT TARGET");
            if (battleManager.GetActiveActor().isControllable())
            {
                EnableInput();
                this.onTargetSelected = onTargetSelected;
                this.waitingForTarget = true;
            }

            if (battleManager.EnemyActors.Count == 1)
            {
                selectedTarget = battleManager.EnemyActors[0];
                waitingForTarget = false;
                this.onTargetSelected();
            }
        }

        public BaseActorBattler GetSelectedTarget()
        {
            UIManager.GetInstance().ChangeStatus(string.Empty);
            var selTarget = selectedTarget;
            selectedTarget = null;
            return selTarget;
        }
    }
}