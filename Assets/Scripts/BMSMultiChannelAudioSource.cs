using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMSMultiChannelAudioSource : MonoBehaviour
{

	[SerializeField]
	private int ChannelLength;
	public int Capacity { get; set; }
	private AudioSource[] AudioSources;

	// Use this for initialization
	private void Awake()
	{
		AudioSources = new AudioSource[ChannelLength];

		for (int i = 0; i < ChannelLength; ++i)
		{
			AudioSources[i] = gameObject.AddComponent<AudioSource>();
			AudioSources[i].loop = false;
			AudioSources[i].playOnAwake = false;
		}
	}

	public void Play(AudioClip clip, float volume = 1.0f)
	{
		foreach(AudioSource a in AudioSources)
		{
			if (a.isPlaying) continue;
			a.clip = clip;
			a.volume = volume;
			a.Play();
			break;
		}
	}

	public void PlayOneShot(AudioClip clip, float volume = 1.0f)
	{
		foreach (AudioSource a in AudioSources)
		{
			if (a.isPlaying) continue;
			a.PlayOneShot(clip, volume);
			break;
		}
	}
}
