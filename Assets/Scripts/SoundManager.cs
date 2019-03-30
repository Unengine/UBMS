using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public GameObject KeySoundObject;
    public MultiChannelAudioSource mulChannel;
    private List<AudioClip> clips;

    public void Awake()
    {
        //audioSource = GetComponent<AudioSource>();
        clips = new List<AudioClip>()
        {
            Capacity = 1300
        };
    }

    // Use this for initialization
    public void AddAudioClips(string path, List<string> soundPathes)
    {
		if (!KeySoundObject)
		{
			Debug.LogError("No KeySoundObject!");
			return;
		}
        foreach(string s in soundPathes)
            clips.Add(Resources.Load<AudioClip>(path + s));

    }

    public void PlayKeySound(int keySound, float volume = 1.0f)
    {
        if (keySound >= clips.Count + 1) return;
        mulChannel.PlayAtSequence(clips[keySound - 1], volume);
        //audioSource.PlayOneShot(clips[keySound - 1]);

        int cnt = 0;
        for (int i = 0; i < mulChannel.ChannelLength; ++i)
            if (mulChannel.GetChannel(i).isPlaying)
                ++cnt;
    }
}
