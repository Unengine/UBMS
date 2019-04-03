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

	private static string[] SoundExtensions;

	public void Awake()
	{
		//audioSource = GetComponent<AudioSource>();
		Pathes = new Dictionary<int, string>();
		Clips = new Dictionary<int, AudioClip>();

		if (SoundExtensions == null)
			SoundExtensions = new string[] { ".ogg", ".wav", ".mp3" };
	}

	public void AddAudioClips()
	{
		StartCoroutine(CAddAudioClips());
	}

    // Use this for initialization
    private IEnumerator CAddAudioClips()
    {
		int extensionFailCount;
		foreach (KeyValuePair<int, string> p in Pathes)
		{
			string url = "file://" + BMSFileSystem.SelectedHeader.ParentPath + @"\";
			WWW www = null;
			extensionFailCount = 0;
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
					Debug.LogWarning($"Failed to read sound data : {www.url}");
					break;
				}
				url.Replace(SoundExtensions[extensionFailCount], SoundExtensions[extensionFailCount + 1]);
				++extensionFailCount;
			}
			while (www.bytes.Length == 0);
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
