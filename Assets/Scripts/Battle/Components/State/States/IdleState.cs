using Assets.Scripts.Pattern;
using UnityEngine;


namespace Assets.Scripts.Battle.Components.State
{
    public class IdleState : IState
    {
        private Battle.Actor actor;
        private SpriteRenderer spriteRenderer;

        public IdleState(Battle.Actor actor)
        {
            this.actor = actor;
            spriteRenderer = actor.GetComponent<SpriteRenderer>();
        }

        public void Enter()
        {
            actor.ai.ChooseAction((chosenAction) => actor.UseAction(chosenAction, () => { actor.state.TransitionTo(actor.state.idleState); }));
        }

        public void Exit()
        {

        }

        public void OnCollisionEnter(Collision collision)
        {
            //throw new System.NotImplementedException();
        }

        public void Update()
        {
            if (actor.isControllable())
                if (Input.GetKey(KeyCode.Space))
                {
                    Time.timeScale = 1f;
                }
                else
                {
                    Time.timeScale = 0f;
                }
        }
    }
}