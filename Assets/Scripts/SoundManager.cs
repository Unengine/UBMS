using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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
			string url = BMSFileSystem.SelectedHeader.ParentPath + @"\";
			UnityWebRequest www = null;
			extensionFailCount = 0;
			AudioType type = AudioType.OGGVORBIS;
			do
			{
				if (File.Exists(url + p.Value + SoundExtensions[extensionFailCount])) break;
				url.Replace(SoundExtensions[extensionFailCount], SoundExtensions[extensionFailCount + 1]);
				++extensionFailCount;
			}
			while (extensionFailCount < SoundExtensions.Length - 1);

			if (string.Compare(SoundExtensions[extensionFailCount], ".wav", true) == 0) type = AudioType.WAV;
			else if (string.Compare(SoundExtensions[extensionFailCount], ".mp3", true) == 0) type = AudioType.MPEG;

			www = UnityWebRequestMultimedia.GetAudioClip(
				"file://" + url + UnityWebRequest.EscapeURL(p.Value + SoundExtensions[extensionFailCount]).Replace('+', ' '), type);
			yield return www.SendWebRequest();

			if (www.downloadHandler.data.Length != 0)
			{
				AudioClip c = DownloadHandlerAudioClip.GetContent(www);
				c.LoadAudioData();
				Clips.Add(p.Key, c);
			}
			else
			{
				Debug.LogWarning($"Failed to read sound data : {www.url}");
			}
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
