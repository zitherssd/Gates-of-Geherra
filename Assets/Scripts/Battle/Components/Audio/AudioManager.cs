using Assets.Scripts.Battle.Actions;
using Assets.Scripts.Utility;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Battle.Components.Audio
{
    public class AudioManager
    {
        private Actor.Actor owner;
        private AudioSource audioSource;

        public AudioManager(Actor.Actor owner)
        {
            this.owner = owner;
            audioSource = owner.GetComponent<AudioSource>();
            owner.OnAfterTakeDamage += PlayDamagedSound;
        }
        public void PlayAudio(string clipName)
        {
            var clip = SoundManager.instance.GetAudioClipByName(clipName);
            audioSource.clip = clip;
            audioSource.Play();
        }

        public void PlayAudioRandomPitch(string clipName, float change)
        {
            var clip = SoundManager.instance.GetAudioClipByName(clipName);
            audioSource.clip = clip;
            audioSource.pitch = Random.Range(1f - change, 1f + change);
            audioSource.Play();
            audioSource.pitch = 1.0f;
        }

        public void PlayDamagedSound(DamageInstance damage)
        {
            PlayAudio("Blow1");
        }

    }
}