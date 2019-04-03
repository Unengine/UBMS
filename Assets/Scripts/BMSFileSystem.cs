using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections.Generic;

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
#else

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
					if (s.Length > 10 && string.Compare(s.Substring(0, 10), "#PLAYLEVEL") == 0)
					{
						int lvl = 0;
						int.TryParse(s.Substring(11), out lvl);
						header.Level = lvl;
					}
					else if (s.Length > 11 && string.Compare(s.Substring(0, 10), "#STAGEFILE") == 0) header.BGImagePath = s.Substring(11, s.Length - 15);
					else if (s.Length >= 9 && string.Compare(s.Substring(0, 9), "#SUBTITLE") == 0) header.Subtitle = s.Substring(10).Trim('[', ']');
					else if (s.Length >= 8 && string.Compare(s.Substring(0, 8), "#PREVIEW") == 0) header.PreviewPath = s.Substring(9, s.Length - 13);
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#PLAYER") == 0) header.Player = s[8] - '0';
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#ARTIST") == 0) header.Artist = s.Substring(8);
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#LNTYPE") == 0) header.LnType |= (Lntype)(1 << (s[8] - '0'));
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#GENRE") == 0) header.Genre = s.Substring(7);
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#TITLE") == 0)
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
						double tot = 160;
						double.TryParse(s.Substring(7), out tot);
						header.Total = tot;
					}
					else if (s.Length >= 5 && string.Compare(s.Substring(0, 5), "#RANK", true) == 0) header.Rank = int.Parse(s.Substring(6));
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 4), "#BPM", true) == 0 && s[4] == ' ') header.Bpm = double.Parse(s.Substring(5));
					else if (s.Length >= 30 && s.CompareTo("*---------------------- MAIN DATA FIELD") == 0) break;
				}
				catch (System.Exception e)
				{
					Debug.Log(e);
					errorFlag = true;
					break;
				}
			}

			if (!errorFlag)
			{
				++PatternCount;
				//Debug.Log($@"file:\\{header.ParentPath}\{header.PreviewPath}");
				if (!string.IsNullOrEmpty(header.PreviewPath) && !UI.PreviewClips.ContainsKey(songinfo))
				{
					StartCoroutine(CLoadPreview(songinfo, header, UI.PreviewClips));
				}
				songinfo.Headers.Add(header);
			}
		}
		songinfo.Headers.Sort();
	}


	public IEnumerator CLoadPreview(BMSSongInfo info, BMSHeader header, Dictionary<BMSSongInfo, AudioClip> dic)
	{
		string[] SoundExtensions = { ".ogg", ".wav", ".mp3" };
		string url = $"file://{header.ParentPath}/{header.PreviewPath}";
		WWW www = null;
		int extensionFailCount = 0;
		do
		{
			www = new WWW(url + WWW.EscapeURL(SoundExtensions[extensionFailCount]).Replace('+', ' '));
			//Debug.Log(www.url);
			if (www.bytes.Length != 0)
			{
				yield return www;
				if (UI.PreviewClips.ContainsKey(info)) break;
				AudioClip c = www.GetAudioClip(false);
				c.LoadAudioData();
				dic.Add(info, c);
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
	}
}
