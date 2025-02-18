using Assets.Scripts.Utility;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Audio
{
    public class AudioManager
    {
        private Battle.Actor owner;
        private AudioSource audioSource;

        public AudioManager(Battle.Actor owner)
        {
            this.owner = owner;
            audioSource = owner.GetComponent<AudioSource>();
            owner.PostureApplied += PlayDamagedSound;
        }
        public void PlayAudio(string clipName)
        {
            var clip = SoundManager.instance.GetAudioClipByName(clipName);
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void PlayDamagedSound(float damage)
        {
            PlayAudio("Blow1");
        }

    }
}