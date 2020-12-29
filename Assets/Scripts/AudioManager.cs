using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	[System.Serializable]
	public class Sound
	{
		public string name;
		[Header("[SPATIAL] Leave It None If Not Spatial")]
		public AudioSource source;
		[Header("[NON-SPATIAL] Adjust If Source Is None")]
		public AudioClip audioClip;
		public AudioMixerGroup mixerGroup;
		public bool playOnAwake;
		public bool loop;
		[Range(0f, 1f)]
		public float volume = 0.75f;
		[Range(-3f, 3f)]
		public float pitch = 1f;
	}

	public static AudioManager instance;
	public AudioMixer audioMixer;
    public Sound[] sounds;
	private bool isMasterMuted;
	private bool isMusicMuted;
	private bool isEffectsMuted;
	private bool isSpeechMuted;

    private void Awake()
    {
		if (instance != null)
			Destroy(gameObject);
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds)
		{
			if (s.source == null)
			{
				s.source = gameObject.AddComponent<AudioSource>();
				s.source.clip = s.audioClip;
				s.source.outputAudioMixerGroup = s.mixerGroup;
				s.source.playOnAwake = s.playOnAwake;
				s.source.loop = s.loop;
				s.source.volume = s.volume;
				s.source.pitch = s.pitch;
			}
		}
	}

	public void Play(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		s.source.Play();
	}

	public void PlayOneShot(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		s.source.PlayOneShot(s.source.clip);
	}

	public void Stop(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		s.source.Stop();
	}

	public void Pause(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		s.source.Pause();
	}

	public void UnPause(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		s.source.UnPause();
	}

	public bool isPlaying(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return false;
		}
		return s.source.isPlaying;
	}

	public void SetVolume(string sound, float value)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		value = Mathf.Clamp(value, 0f, 1f);
		s.source.volume = value;
	}

	public void SetPitch(string sound, float value)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		value = Mathf.Clamp(value, -3f, 3f);
		s.source.pitch = value;
	}

	public void SetClip(string sound, AudioClip clip)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		s.source.clip = clip;
	}

	public void SetVolumeMaster(float value)
	{
		value = Mathf.Clamp(value, -80f, 0f);
		audioMixer.SetFloat("MasterVolume", value);
	}

	public void SetVolumeMusic(float value)
	{
		value = Mathf.Clamp(value, -80f, 0f);
		audioMixer.SetFloat("MusicVolume", value);
	}

	public void SetVolumeEffects(float value)
	{
		value = Mathf.Clamp(value, -80f, 0f);
		audioMixer.SetFloat("EffectsVolume", value);
	}

	public void SetVolumeSpeech(float value)
	{
		value = Mathf.Clamp(value, -80f, 0f);
		audioMixer.SetFloat("SpeechVolume", value);
	}

	public void ToggleMuteMaster()
	{
		if (isMasterMuted)
		{
			isMasterMuted = false;
			audioMixer.SetFloat("MasterVolume", 0f);
		}
		else
		{
			isMasterMuted = true;
			audioMixer.SetFloat("MasterVolume", -80f);
		}
	}

	public void ToggleMuteMusic()
	{
		if (isMusicMuted)
		{
			isMusicMuted = false;
			audioMixer.SetFloat("MusicVolume", 0f);
		}
		else
		{
			isMusicMuted = true;
			audioMixer.SetFloat("MusicVolume", -80f);
		}
	}

	public void ToggleMuteEffects()
	{
		if (isEffectsMuted)
		{
			isEffectsMuted = false;
			audioMixer.SetFloat("EffectsVolume", 0f);
		}
		else
		{
			isEffectsMuted = true;
			audioMixer.SetFloat("EffectsVolume", -80f);
		}
	}

	public void ToggleMuteSpeech()
	{
		if (isSpeechMuted)
		{
			isSpeechMuted = false;
			audioMixer.SetFloat("SpeechVolume", 0f);
		}
		else
		{
			isSpeechMuted = true;
			audioMixer.SetFloat("SpeechVolume", -80f);
		}
	}

	public void MuteMaster()
	{
		audioMixer.SetFloat("MasterVolume", -80f);
	}

	public void UnmuteMaster()
	{
		audioMixer.SetFloat("MasterVolume", 0f);
	}

	public void MuteMusic()
	{
		audioMixer.SetFloat("MusicVolume", -80f);
	}

	public void UnmuteMusic()
	{
		audioMixer.SetFloat("MusicVolume", 0f);
	}

	public void MuteEffects()
	{
		audioMixer.SetFloat("EffectsVolume", -80f);
	}

	public void UnmuteEffects()
	{
		audioMixer.SetFloat("EffectsVolume", 0f);
	}

	public void MuteSpeech()
	{
		audioMixer.SetFloat("SpeechVolume", -80f);
	}

	public void UnmuteSpeech()
	{
		audioMixer.SetFloat("SpeechVolume", 0f);
	}
}
