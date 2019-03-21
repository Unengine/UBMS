using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public GameObject KeySoundObject;
    public AudioSource audioSource;
    public AudioClip bgSound;
    public MultiChannelAudioSource mulChannel;
    private List<AudioClip> clips;
    //private List<AudioSource> audioSources;
    [SerializeField]

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
        if (!KeySoundObject) return;
        foreach(string s in soundPathes)
            clips.Add(Resources.Load<AudioClip>(path + s));

    }

    int idx = 0;
    public void PlayKeySound(int keySound, float volume)
    {
        if (keySound >= clips.Count + 1) return;
        mulChannel.PlayAtSequence(clips[keySound - 1], volume);
        //audioSource.PlayOneShot(clips[keySound - 1]);

        int cnt = 0;
        for (int i = 0; i < mulChannel.ChannelLength; ++i)
            if (mulChannel.GetChannel(i).isPlaying)
                ++cnt;
    }

    public void PlayBG()
    {
        audioSource.PlayOneShot(bgSound);
    }
}
