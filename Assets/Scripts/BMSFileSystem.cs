using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System.Text;

public class BMSFileSystem : MonoBehaviour {

	public static string[] Directories;
	public static BMSSongInfo[] Songinfos;
	//public Button[] SelButtons;
	public static BMSHeader SelectedHeader;
	public static string SelectedPath;
	public int PatternCount = 0;

	[SerializeField]
	private RectTransform ScrollViewport;
	[SerializeField]
	private GameObject ButtonPrefab;
	private static string RootPath;

	private void Awake () {
		if (!string.IsNullOrEmpty(RootPath)) return;

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
		DrawUI();
	}

	private void DrawUI()
	{
		int i = 1;
		foreach (BMSSongInfo s in Songinfos)
		{
			foreach (BMSHeader h in s.Headers)
			{
				GameObject t;
				(t = Instantiate(ButtonPrefab, ScrollViewport)).transform.localPosition = new Vector3(300, (50 * PatternCount) - (100 * i++) - 2450);
				t.GetComponentInChildren<Text>().text = h.Title;
				t.GetComponent<Button>().onClick.AddListener(() =>
				{
					SelectedHeader = h;
					SelectedPath = h.ParentPath;
					UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
				});
			}
		}

		ScrollViewport.sizeDelta = new Vector2(0, 100 * PatternCount);
	}

	private void ParseHeader(string dir, out BMSSongInfo songinfo)
	{
		songinfo = new BMSSongInfo();

		var Files = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
			.Where(s => s.EndsWith(".bms") || s.EndsWith(".bme") || s.EndsWith(".bml")).ToArray();

		if (Files.Length == 0) return;
		foreach(string path in Files)
		{
			StreamReader reader = new StreamReader(path);
			BMSHeader header = new BMSHeader();
			string s;
			header.ParentPath = path;

			bool errorFlag = false;
			while((s = reader.ReadLine()) != null)
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
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#PLAYER") == 0) header.Player = s[8] - '0';
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#ARTIST") == 0) header.Artist = s.Substring(8, s.Length - 8);
					else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#LNTYPE") == 0) header.LnType |= (BMSHeader.Lntype)(1 << (s[8] - '0'));
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#GENRE") == 0) header.Genre = s.Substring(7);
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#TITLE") == 0)
					{
						header.Title = s.Substring(7);
						if (!string.IsNullOrEmpty(header.Title))
						{
							int idx;
							if ((idx = header.Title.LastIndexOf('[')) >= 0)
								songinfo.SongName = header.Title.Remove(idx);
							else
								songinfo.SongName = header.Title;
						}
					}
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#TOTAL") == 0)
					{
						double tot = 160;
						double.TryParse(s.Substring(7), out tot);
						header.Total = tot;
					}
					else if (s.Length >= 5 && string.Compare(s.Substring(0, 5), "#RANK") == 0) header.Rank = int.Parse(s.Substring(6));
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 4), "#BPM") == 0) header.Bpm = double.Parse(s.Substring(5));
					else if (s.Length >= 6 && string.Compare(s.Substring(0, 4), "#WAV") == 0) break;
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
				songinfo.Headers.Add(header);
			}
		}
	}
}
