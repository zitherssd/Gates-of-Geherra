using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class ParticleController : MonoBehaviour
    {
        private ParticleSystem tp_effect;
        private ParticleSystem run_effect;
        private ParticleSystem guide;
        private Transform holder;

        private void Start()
        {
            holder = gameObject.transform.Find("Effects");
            tp_effect = holder.Find("Tp effect").GetComponent<ParticleSystem>();
            run_effect = holder.Find("Run effect").GetComponent<ParticleSystem>();
        }
        // Update is called once per frame
        public void Play_TpEffect()
        {
            tp_effect.Play();
        }

        public void Play_RunEffect()
        {
            run_effect.Play();
        }
    }
}