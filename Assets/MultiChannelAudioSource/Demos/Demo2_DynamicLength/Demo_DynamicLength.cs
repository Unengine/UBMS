using UnityEngine;
using UnityEngine.UI;

public class Demo_DynamicLength : MonoBehaviour {
	[HideInInspector] public MultiChannelAudioSource audioSource;
	public Slider channelSlider;
	public Text displayText;

	void Start() {
		audioSource = GetComponent<MultiChannelAudioSource>();
		channelSlider.onValueChanged.AddListener(UpdateSlider);
	}

	void UpdateSlider(float value) {
		displayText.text = string.Format("Channels: {0}", value.ToString());
		audioSource.ChannelLength = (int) value;
	}
}
