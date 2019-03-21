using UnityEngine;
using UnityEngine.Serialization;

public class MultiChannelAudioSource : MonoBehaviour {
	[SerializeField] private int m_StartingChannels = 2;
	private bool m_Initialized = false;
	private int m_ChannelLength = 0;
	private int m_CurrentChannelIdx = 0;
	private AudioSource[] audioSources;

	public int ChannelLength {
		get {
			return m_ChannelLength;
		}
		set {
			if(value == 0) {
				if(audioSources.Length > 0) {
					for(int i = 0; i < audioSources.Length; i++) {
						Destroy(audioSources[i]);
					}
				}

				audioSources = new AudioSource[0];
			}
			else if(value > m_ChannelLength) {
				int moreChannelsToCreate = value - m_ChannelLength;

				AudioSource[] newAudioSources = new AudioSource[audioSources.Length + moreChannelsToCreate];

				for(int i = 0; i < audioSources.Length; i++) {
					newAudioSources[i] = audioSources[i];
				}

				for(int i = 0; i < moreChannelsToCreate; i++) {
					newAudioSources[i + audioSources.Length] = gameObject.AddComponent<AudioSource>();
				}

				audioSources = newAudioSources;
			}
			else if(value < m_ChannelLength) {
				// Create new Shrinked AudioSource Array
				AudioSource[] newAudioSources = new AudioSource[value];

				// Copy to new AudioSources
				for(int i = 0; i < value; i++) {
					newAudioSources[i] = audioSources[i];
				}

				// Remove AudioSources out of new Array
				for(int i = value; i < audioSources.Length; i++) {
					Destroy(audioSources[i]);
				}
			}

			m_ChannelLength = value;
		}
	}

	public bool Initialized {
		get {
			return m_Initialized;
		}
	}

	void Start() {
		audioSources = new AudioSource[0];
		ChannelLength = m_StartingChannels;

		m_Initialized = true;
	}

	public AudioSource GetChannel(int idx) {
		return audioSources[idx];
	}

	public void Play(int idx, AudioClip audioClip) {
		audioSources[idx].clip = audioClip;
		audioSources[idx].Play();
	}

	public void PlayOneShot(int idx, AudioClip audioClip) {
		audioSources[idx].PlayOneShot(audioClip);
	}

	public void Stop(int idx) {
		audioSources[idx].Stop();
	}

	public void PlayAtSequence(AudioClip audioClip, float volume = 1.0f) {
        audioSources[m_CurrentChannelIdx].volume = volume;
		audioSources[m_CurrentChannelIdx].clip = audioClip;
		audioSources[m_CurrentChannelIdx].Play();

		if(++m_CurrentChannelIdx >= m_ChannelLength) {
			m_CurrentChannelIdx = 0;
		}
	}

	public void PlayAtSequence(AudioClip audioClip, int channelCount) {
		audioSources[m_CurrentChannelIdx].clip = audioClip;
		audioSources[m_CurrentChannelIdx].Play();

		m_CurrentChannelIdx++;

		if(m_CurrentChannelIdx >= m_ChannelLength || m_CurrentChannelIdx >= channelCount) {
			m_CurrentChannelIdx = 0;
		}
	}

	public void PlayOneShotAtSequence(AudioClip audioClip) {
		audioSources[m_CurrentChannelIdx].PlayOneShot(audioClip);

		if(++m_CurrentChannelIdx >= m_ChannelLength) {
			m_CurrentChannelIdx = 0;
		}
	}

	public void PlayOneShotAtSequence(AudioClip audioClip, int channelCount) {
		audioSources[m_CurrentChannelIdx].PlayOneShot(audioClip);

		m_CurrentChannelIdx++;

		if(m_CurrentChannelIdx >= m_ChannelLength || m_CurrentChannelIdx >= channelCount) {
			m_CurrentChannelIdx = 0;
		}
	}
}
