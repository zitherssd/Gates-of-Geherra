using Assets.Scripts.Utility;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class RestAreaManager : MonoBehaviour
    {
        public Transform RestArea;

        public void Enter()
        {
            SlowdownManager.instance.ExitStateSlowdown();
            GameFlowManager.instance.playerActor.transform.position = RestArea.position;
            GameFlowManager.instance.playerActor.PlayAnimation("Fire");
            SoundManager.instance.PlayMusicRest();

            UIManager.instance.Fade(false, () =>
            {
                UIManager.instance.ShowRestingUI();
            });

        }
    }
}


