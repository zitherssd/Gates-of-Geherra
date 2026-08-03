using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utility
{
	public class SoundManager : MonoBehaviour
	{

		public AudioSource efxSource;
		public AudioSource musicSource;
		public List<AudioClip> music;
		public List<AudioClip> restMusic;
		public List<AudioClip> soundEffects;
		public static SoundManager instance = null;
		private float fadeOutDuration = 1.0f;

		public float lowPitchRange = 0.95f;
		public float highPitchRange = 1.05f;

		void Awake()
		{
			if (instance == null)
				instance = this;
			else if (instance != this)
				Destroy(gameObject);

		}
		public void PlayMusic(AudioClip clip)
        {
			StopAllCoroutines();
			System.Random random = new System.Random();
			int randomIndex = random.Next(music.Count);
			musicSource.volume = 0.8f; // Reset volume to 1.0f before playing
			musicSource.clip = music[randomIndex];
			musicSource.Play();
        }

		public void PlaySE(string name)
		{
            efxSource.clip = SoundManager.instance.GetAudioClipByName(name);
			efxSource.Play();
        }

		public void PlayMusicRest()
		{
            StopAllCoroutines();
            System.Random random = new System.Random();
            int randomIndex = random.Next(restMusic.Count);
            //musicSource.volume = 0.5f; // Reset volume to 1.0f before playing
            musicSource.clip = restMusic[randomIndex];
			musicSource.Play();

        }

		public void PlaySingle(AudioClip clip)
		{
			efxSource.clip = clip;
			efxSource.Play();
		}

		public void RandomizeSfx(params AudioClip[] clips)
		{
			int randomIndex = Random.Range(0, clips.Length);
			float randomPitch = Random.Range(lowPitchRange, highPitchRange);

			efxSource.pitch = randomPitch;
			efxSource.clip = clips[randomIndex];
			efxSource.Play();
		}

		public AudioClip GetAudioClipByName(string name)
        {

			return soundEffects.First(item => item.name == name);
        }

        public void FadeOutMusic(float duration = 1f)
        {
            StartCoroutine(FadeOutMusicCoroutine(duration));
        }

        private IEnumerator FadeOutMusicCoroutine(float duration)
        {
            if (musicSource == null || !musicSource.isPlaying)
                yield break;

            float startVolume = musicSource.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume; // reset for later
        }
    }
}