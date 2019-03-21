using UnityEngine;

public class Demo_AudioSource : MonoBehaviour {
	private AudioSource audioSource;
	public AudioClip audioClip;
	public float nextPlayTime = 0.1f;

	System.Collections.IEnumerator Start() {
		audioSource = GetComponent<AudioSource>();

		while(true) {
			audioSource.PlayOneShot(audioClip);
			yield return new WaitForSeconds(nextPlayTime);
		}
	}
}
