using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(MultiChannelAudioSource))]
public class AudioSourceProxy : MonoBehaviour {
	private MultiChannelAudioSource multiChannelAudioSource;
	public AudioSource source;

	// Simulate properties in AudioSource
	public AudioMixerGroup output;
	public bool bypassEffects = false;
	public bool bypassListenerEffects = false;
	public bool bypassReverbZones = false;
	[Range(0, 256)] public int priority = 128;
	[Range(0, 1)] public float volume = 1.0f;
	[Range(-3, 3)] public float pitch = 1;
	[Range(-1, 1)] public float stereoPan = 0;
	[Range(0, 1)] public float spatialBlend = 0;
	[Range(0, 1.1f)] public float reverbZoneMix = 1;

	IEnumerator Start() {
		multiChannelAudioSource = GetComponent<MultiChannelAudioSource>();

		// Wait Until MultiChannelAudioSource is Ready
		while(!multiChannelAudioSource.Initialized) {
			yield return null;
		}

		UpdateChannels();
	}

	public void UpdateChannels() {
		for(int i = 0; i < multiChannelAudioSource.ChannelLength; i++) {
			AudioSource channel = multiChannelAudioSource.GetChannel(i);

			// If has source, update from it.
			if(source) {
				channel.outputAudioMixerGroup = source.outputAudioMixerGroup;
				channel.bypassEffects = source.bypassEffects;
				channel.bypassListenerEffects = source.bypassListenerEffects;
				channel.bypassReverbZones = source.bypassReverbZones;

				channel.priority = source.priority;
				channel.volume = source.volume;
				channel.pitch = source.pitch;
				channel.panStereo = source.panStereo;
				channel.spatialBlend = source.spatialBlend;
				channel.reverbZoneMix = source.reverbZoneMix;
			}

			// Else, update from internal values
			else {
				if(output) {
					channel.outputAudioMixerGroup = output;
				}
				else {
					channel.outputAudioMixerGroup = null;
				}

				channel.bypassEffects = bypassEffects;
				channel.bypassListenerEffects = bypassListenerEffects;
				channel.bypassReverbZones = bypassReverbZones;

				channel.priority = priority;
				channel.volume = volume;
				channel.pitch = pitch;
				channel.panStereo = stereoPan;
				channel.spatialBlend = spatialBlend;
				channel.reverbZoneMix = reverbZoneMix;
			}
		}
	}
}
