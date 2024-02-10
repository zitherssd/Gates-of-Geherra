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

			DontDestroyOnLoad(gameObject);
		}
		public void PlayMusic(AudioClip clip)
        {
			System.Random random = new System.Random();
			int randomIndex = random.Next(music.Count);
			musicSource.volume = 1.0f; // Reset volume to 1.0f before playing
			musicSource.clip = music[randomIndex];
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

		public void FadeOutMusic()
		{
			StartCoroutine(FadeOut(musicSource, fadeOutDuration));
		}

		private IEnumerator FadeOut(AudioSource audioSource, float duration)
		{
			float startVolume = audioSource.volume;

			for (float t = 0; t < duration; t += Time.deltaTime)
			{
				audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
				yield return null;
			}

			audioSource.volume = 0;
			audioSource.Stop();
		}

	}
}