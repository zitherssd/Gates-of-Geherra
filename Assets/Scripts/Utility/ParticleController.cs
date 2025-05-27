using Assets.Scripts.Battle.Actor;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class ParticleController : MonoBehaviour
    {
        private ParticleSystem tp_effect;
        private ParticleSystem run_effect;
        private ParticleSystem jump_effect;
        private ParticleSystem guide;
        private Transform holder;
        private Actor actor;

        private void Start()
        {
            holder = gameObject.transform.Find("Effects");
            tp_effect = holder.Find("Tp effect").GetComponent<ParticleSystem>();
            run_effect = holder.Find("Run effect").GetComponent<ParticleSystem>();
            jump_effect = holder.Find("Jump").GetComponent<ParticleSystem>();
            actor = GetComponent<Actor>();
        }
        // Update is called once per frame
        public void Play_TpEffect()
        {
            tp_effect.Play();
        }

        public void Play_RunEffect()
        {
            actor.audio.PlayAudioRandomPitch("Footstep", 0.2f);
            run_effect.Play();
        }
        public void TestME(string test)
        {
            run_effect.Play();

        }
    }
}