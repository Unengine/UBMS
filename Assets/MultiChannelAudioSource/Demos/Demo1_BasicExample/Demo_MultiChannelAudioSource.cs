using UnityEngine;

public class Demo_MultiChannelAudioSource : MonoBehaviour {
	private MultiChannelAudioSource audioSource;
	public AudioClip audioClip;
	public float nextPlayTime = 0.1f;

	System.Collections.IEnumerator Start() {
		audioSource = GetComponent<MultiChannelAudioSource>();

		// Wait until MultiChannelAudioSource generates internal audio sources completely
		yield return null;

		while(true) {
			audioSource.PlayAtSequence(audioClip);
			yield return new WaitForSeconds(nextPlayTime);
		}
	}
}
