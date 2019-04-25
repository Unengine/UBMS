using System.Collections;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine.Networking;

public class BMSFileSystem : MonoBehaviour {

	public static string[] Directories;
	public static BMSSongInfo[] Songinfos;
	//public Button[] SelButtons;
	public static BMSHeader SelectedHeader;
	public static string SelectedPath;
	public static int PatternCount = 0;

	[SerializeField]
	private SelUIManager UI;
	private static string RootPath;

	private void Awake() {
		if (string.IsNullOrEmpty(RootPath))
		{
#if UNITY_EDITOR
			RootPath = @"D:\BMSFiles\";
#elif DEVELOPMENT_BUILD
			RootPath = @"D:\BMSFiles\";
#else
			RootPath = $@"{Directory.GetParent(Application.dataPath)}\BMSFiles";
#endif
			Directories = Directory.GetDirectories(RootPath);
			Songinfos = new BMSSongInfo[Directories.Length];
			for (int i = 0; i < Directories.Length; ++i)
			{
				ParseHeader(Directories[i], out Songinfos[i]);
			}
		}

		UI.DrawSongUI(Songinfos);
		//UI.DrawPatternUI(Songinfos, PatternCount);
	}

	private void ParseHeader(string dir, out BMSSongInfo songinfo)
	{
		songinfo = new BMSSongInfo();

		var Files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
			.Where(s => s.EndsWith(".bms") || s.EndsWith(".bme") || s.EndsWith(".bml")).ToArray();

		if (Files.Length == 0) return;
		foreach (string path in Files)
		{
			StreamReader reader = new StreamReader(path, Encoding.GetEncoding(932));
			BMSHeader header = new BMSHeader();
			string s;
			header.Path = path;
			header.ParentPath = Directory.GetParent(path).ToString();

			bool errorFlag = false;
			while ((s = reader.ReadLine()) != null)
			{
				if (s.Length <= 3) continue;

				try
				{
					if (s.Length > 10 && string.Compare(s.Substring(0, 10), "#PLAYLEVEL", true) == 0)
					{
						int lvl = 0;
						int.TryParse(s.Substring(11), out lvl);
						header.Level = lvl;
					}
					else if (s.Length > 11 && string.Compare(s.Substring(0, 10), "#STAGEFILE", true) == 0) header.StagefilePath = s.Substring(11);
					else if (s.Length >= 9 && string.Compare(s.Substring(0, 9), "#SUBTITLE", true) == 0) header.Subtitle = s.Substring(10).Trim('[', ']');
					else if (s.Length >= 8 && string.Compare(s.Substring(0, 8), "#PREVIEW", true) == 0) header.PreviewPath = s.Substring(9, s.Length - 13);
					else if (s.Length >= 8 && string.Compare(s.Substring(0, 8), "#BACKBMP", true) == 0) header.BackbmpPath = s.Substring(9);
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#PLAYER", true) == 0) header.Player = s[8] - '0';
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#ARTIST", true) == 0) header.Artist = s.Substring(8);
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#BANNER", true) == 0) header.BannerPath = s.Substring(8);
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#LNTYPE", true) == 0) header.LnType |= (Lntype)(1 << (s[8] - '0'));
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#GENRE", true) == 0) header.Genre = s.Substring(7);
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#TITLE", true) == 0)
					{
						header.Title = s.Substring(7);
						if (!string.IsNullOrEmpty(header.Title))
						{
							int idx;
							if ((idx = header.Title.LastIndexOf('[')) >= 0)
							{
								string name = header.Title.Remove(idx);
								if (string.IsNullOrEmpty(songinfo.SongName) || songinfo.SongName.Length > name.Length)
									songinfo.SongName = name;
								header.Subtitle = header.Title.Substring(idx).Trim('[', ']');
							}
							else
							{
								if (string.IsNullOrEmpty(songinfo.SongName) || songinfo.SongName.Length > header.Title.Length)
									songinfo.SongName = header.Title;
							}
						}
					}
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#TOTAL", true) == 0)
					{
						float tot = 160;
						float.TryParse(s.Substring(7), out tot);
						header.Total = tot;
					}
					else if (s.Length >= 5 && string.Compare(s.Substring(0, 5), "#RANK", true) == 0) header.Rank = int.Parse(s.Substring(6));
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 4), "#BPM", true) == 0 && s[4] == ' ') header.Bpm = double.Parse(s.Substring(5));
					else if (s.Length >= 30 && s.CompareTo("*---------------------- MAIN DATA FIELD") == 0) break;
				}
				catch (System.Exception e)
				{
					Debug.LogWarning("error parsing " + s + "\n" + e);
					errorFlag = true;
					break;
				}
			}

			if (!errorFlag)
			{
				++PatternCount;
				//Debug.Log($@"file:\\{header.ParentPath}\{header.PreviewPath}");
				if (!string.IsNullOrEmpty(header.PreviewPath) && !SelUIManager.PreviewClips.ContainsKey(songinfo))
				{
					StartCoroutine(CLoadPreview(songinfo, header, SelUIManager.PreviewClips));
				}
				songinfo.Headers.Add(header);
			}
		}
		songinfo.Headers.Sort();
	}


	public IEnumerator CLoadPreview(BMSSongInfo info, BMSHeader header, Dictionary<BMSSongInfo, AudioClip> dic)
	{
		string[] SoundExtensions = { ".ogg", ".wav", ".mp3" };
		AudioType type = AudioType.OGGVORBIS;
		string url = $@"{header.ParentPath}\{header.PreviewPath}";
		UnityWebRequest www = null;
		int extensionFailCount = 0;
		do
		{
			if (File.Exists(url + SoundExtensions[extensionFailCount])) break;
			url.Replace(SoundExtensions[extensionFailCount], SoundExtensions[extensionFailCount + 1]);
			++extensionFailCount;
		}
		while (extensionFailCount < SoundExtensions.Length - 1);
		//clips.Add(Resources.Load<AudioClip>(path + s));

		if (string.Compare(SoundExtensions[extensionFailCount], ".wav", true) == 0) type = AudioType.WAV;
		else if (string.Compare(SoundExtensions[extensionFailCount], ".mp3", true) == 0) type = AudioType.MPEG;

		www = UnityWebRequestMultimedia.GetAudioClip("file://" + url + SoundExtensions[extensionFailCount], type);
		yield return www.SendWebRequest();
		if (www.downloadHandler.data.Length != 0)
		{
			AudioClip c = DownloadHandlerAudioClip.GetContent(www);
			c.LoadAudioData();

			if (!SelUIManager.PreviewClips.ContainsKey(info))
				SelUIManager.PreviewClips.Add(info, c);
		}
		else
		{
			Debug.LogWarning($"Failed to read sound data : {www.url}");
		}
	}
}
