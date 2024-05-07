using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

namespace Assets
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager instance = null;

        private BattleManager battleManager;
        private Action onTurnEnd = null;
        private Action<Actor> onTargetSelected = null;
        private Action<Vector2> onSwipeGot = null;
        private Action<Vector3> onTargetPointSelected = null;
        public OnScreenStick onScreenStick;
        public float SliderValue = 1f;
        public Vector2 StickValue = Vector2.zero;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }
        private void Start()
        {
            battleManager = BattleManager.instance;   
        }

        public void SetSliderValue(System.Single value)
        {
            SliderValue = value;
        }
        public void SetStickValue(Vector2 value)
        {
            StickValue = value;
        }


        public void WaitForTurn(Action onTurnEnd)
        {
            InputHandler.instance.enabled = false;
            InputHandler.instance.enabled = true;

            //InputHandler.instance.OnSwipe += MoveActor;
            //InputHandler.instance.OnHold += EndTurn;

            this.onTurnEnd = onTurnEnd;
            var skillsToDraw = battleManager.GetActiveActor().GetValidContinueComboSkills();
            if (skillsToDraw.Count > 0)
                Time.timeScale = 0f;

            UIManager.GetInstance().DrawActionsAndWaitForSelectionOrNull(skillsToDraw, selectedSkill =>    //one shot option for the skill
            {
                InputHandler.instance.OnSwipe -= MoveActor;
                InputHandler.instance.OnHold -= EndTurn;

                if (selectedSkill == null)
                {
                    Time.timeScale = 1f;
                    onTurnEnd();
                }
                else
                    battleManager.GetActiveActor().UseAction(selectedSkill, onTurnEnd);
            });
        }
        private void MoveActor(Vector2 delta)
        {
            InputHandler.instance.OnSwipe -= MoveActor;
            InputHandler.instance.OnHold -= EndTurn;
            ButtonHandler.KillAll();

            battleManager.GetActiveActor().MoveRelativeToCamera(delta.normalized, () =>
            {
                //InputHandler.instance.OnHold += EndTurn;

                if  (battleManager.GetActiveActor().GetValidStartComboSkills().Count == 0) { EndTurn(Vector2.zero); return; }; //Automatically end turn if no valid skills

                var skillsToDraw = battleManager.GetActiveActor().GetValidStartComboSkills();
                UIManager.GetInstance().DrawActionsAndWaitForSelectionOrNull(skillsToDraw, selectedSkill =>    //one shot option for the skill
                {
                    InputHandler.instance.OnHold -= EndTurn;

                    if (selectedSkill == null)
                        onTurnEnd();
                    else
                        battleManager.GetActiveActor().UseAction(selectedSkill, onTurnEnd);
                });
            });
        }
        private void EndTurn(Vector2 delta)
        {
            InputHandler.instance.OnHold -= EndTurn;
            ButtonHandler.KillAll();
            onTurnEnd.Invoke();
        }


        public void WaitForSwipe(Action<Vector2> onSwipeGot)
        {
            Time.timeScale = 0f;
            UIManager.GetInstance().ChangeStatus("SWIPE TO CHOOSE DIRECTION");
            this.onSwipeGot = onSwipeGot;
            InputHandler.instance.OnSwipe += OnSwipeRecieved;
        }
        private void OnSwipeRecieved(Vector2 direction)
        {
            UIManager.GetInstance().ChangeStatus(string.Empty);
            InputHandler.instance.OnSwipe -= OnSwipeRecieved;
            Time.timeScale = 1f;
            onSwipeGot?.Invoke(direction);
        }


        public void WaitForTargetActor(Action<Actor> onTargetSelected)
        {
            UIManager.GetInstance().ChangeStatus("SELECT TARGET");

            //If there's only one enemy actor automatically select it as target
            if (battleManager.EnemyActors.Count == 1)
            {
                var target = battleManager.EnemyActors[0];
                onTargetSelected?.Invoke(target);
                return;
            }

            this.onTargetSelected = onTargetSelected;
            InputHandler.instance.OnClick += OnTargetRecieved;
        }
        private void OnTargetRecieved(Vector2 clickPosition)
        {
            Ray raycast = Camera.main.ScreenPointToRay(clickPosition);
            if (Physics.Raycast(raycast, out RaycastHit raycastHit) && raycastHit.collider.name == "EnemyBattler")
            {
                UIManager.GetInstance().ChangeStatus(string.Empty);
                var target = raycastHit.collider.GetComponentInChildren<Actor>();
                InputHandler.instance.OnClick -= OnTargetRecieved;
                onTargetSelected?.Invoke(target);
            }
        }


        public void WaitForTargetPoint(Action<Vector3> onTargetPointSelected)
        {
            UIManager.GetInstance().ChangeStatus("SELECT TARGET POINT");
            this.onTargetPointSelected = onTargetPointSelected;
            InputHandler.instance.OnClick += OnTargetRecieved;
        }
        private void OnTargetPointRecieved(Vector2 clickPosition)
        {
            UIManager.GetInstance().ChangeStatus(string.Empty);
            Ray raycast = Camera.main.ScreenPointToRay(clickPosition);
            if (Physics.Raycast(raycast, out RaycastHit raycastHit))
            {
                InputHandler.instance.OnClick -= OnTargetPointRecieved;
                onTargetPointSelected?.Invoke(raycastHit.point);
            }
        }


    }
}