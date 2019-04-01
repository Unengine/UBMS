using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	public bool IsPrepared { get; set; } = false;
    public GameObject KeySoundObject;
    public MultiChannelAudioSource mulChannel;
	public BMSMultiChannelAudioSource Src;
    public Dictionary<int, string> Pathes { get; set; }
	public Dictionary<int, AudioClip> Clips { get; set; }

	private string[] SoundExtensions;

	public void Awake()
	{
		//audioSource = GetComponent<AudioSource>();
		Pathes = new Dictionary<int, string>();
		Clips = new Dictionary<int, AudioClip>();
		SoundExtensions = new string[] { ".ogg", ".wav", ".mp3" };
	}

	public void AddAudioClips()
	{
		StartCoroutine(CAddAudioClips());
	}

    // Use this for initialization
    public IEnumerator CAddAudioClips()
    {
		//if (!KeySoundObject)
		//{
		//	Debug.LogError("No KeySoundObject!");
		//	return;
		//}

		//Debug.Log("file://" + Directory.GetParent(BMSFileSystem.SelectedPath) + "/");

		int extensionFailCount = 0;
		foreach (KeyValuePair<int, string> p in Pathes)
		{
			string url = "file://" + Directory.GetParent(BMSFileSystem.SelectedPath) + @"\";
			WWW www = null;
			do
			{
				www = new WWW(url + WWW.EscapeURL(p.Value + SoundExtensions[extensionFailCount]).Replace('+', ' '));
				//Debug.Log(www.url);
				if (www.bytes.Length != 0)
				{
					yield return www;
					AudioClip c = www.GetAudioClip(false);
					c.LoadAudioData();
					Clips.Add(p.Key, c);
					break;
				}
				if (extensionFailCount >= SoundExtensions.Length - 1)
				{
					Debug.LogError("Failed to read sound data");
					break;
				}
				url.Replace(SoundExtensions[extensionFailCount], SoundExtensions[extensionFailCount + 1]);
				++extensionFailCount;
			}
			while (www.bytes.Length == 0);
			extensionFailCount = 0;
			//clips.Add(Resources.Load<AudioClip>(path + s));
		}

		IsPrepared = true;
    }

    public void PlayKeySound(int key, float volume = 1.0f)
    {
		if (key == 0) return;
		if (Clips.ContainsKey(key))
			Src.PlayOneShot(Clips[key], volume);
    }
}
