using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public class ParticleController : MonoBehaviour
    {
        private ParticleSystem tp_effect;
        private ParticleSystem run_effect;


        private void Start()
        {
            tp_effect = gameObject.transform.Find("Tp effect").GetComponent<ParticleSystem>();
            run_effect = gameObject.transform.Find("Run effect").GetComponent<ParticleSystem>();
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