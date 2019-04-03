using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SelUIManager : MonoBehaviour {

	public static float ScrollValue = 1;
	public static Dictionary<BMSSongInfo, AudioClip> PreviewClips;
	public Scrollbar Scroll;
	public Text SpeedText;

	[SerializeField]
	private GameObject ButtonPrefab;
	[SerializeField]
	private RectTransform SongViewport;
	[SerializeField]
	private RectTransform PatternViewport;
	[SerializeField]
	private RawImage Banner;
	[SerializeField]
	private Text TitleText;
	[SerializeField]
	private Text SubTitleText;
	[SerializeField]
	private Text GenreText;
	[SerializeField]
	private Text BPMText;
	[SerializeField]
	private GameObject InformText;
	[SerializeField]
	private AudioSource Preview;

	private GameObject[] PatternButtons;
	private bool IsReady = false;

	// Use this for initialization
	void Awake () {
		if (PreviewClips == null)
			PreviewClips = new Dictionary<BMSSongInfo, AudioClip>();
		UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##"));
		Scroll.value = ScrollValue;
		Screen.SetResolution(1280, 720, true);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

		ScrollValue = Scroll.value;
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (BMSGameManager.Speed > 1f)
			{
				BMSGameManager.Speed -= 0.5f;

				UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##") +
					(BMSFileSystem.SelectedHeader != null ?
					$" ({(BMSGameManager.Speed * BMSFileSystem.SelectedHeader.Bpm).ToString("0")})" : string.Empty));
			}
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			BMSGameManager.Speed += 0.5f;
			UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##") +
				(BMSFileSystem.SelectedHeader != null ?
				$" ({(BMSGameManager.Speed * BMSFileSystem.SelectedHeader.Bpm).ToString("0")})" : string.Empty));
		}
		else if (IsReady && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
		{
			Preview.Stop();
			UnityEngine.SceneManagement.SceneManager.LoadScene(1);
		}
	}

	public void UpdateText(Text text, string str) => text.text = str;

	public void DrawSongUI(BMSSongInfo[] songinfos)
	{
		int i = 0;
		SongViewport.sizeDelta = new Vector2(0, 71 * songinfos.Length);
		foreach (BMSSongInfo s in songinfos)
		{
			if (s.Headers.Count == 0) continue;
			GameObject t;
			(t = Instantiate(ButtonPrefab, SongViewport)).transform.localPosition = new Vector3(300, 30 - (70 * ++i));   //2450
			t.GetComponentInChildren<Text>().text = s.SongName;
			t.GetComponent<Button>().onClick.AddListener(() =>
			{
				if (PatternButtons != null)
					foreach (GameObject g in PatternButtons)
						if (g != null)
							Destroy(g);
				DrawPatternUI(s, s.Headers.Count);
			});
		}
	}

	public void DrawPatternUI(BMSSongInfo songinfo, int patternCount)
	{
		int i = 0;
		
		PatternButtons = new GameObject[songinfo.Headers.Count];
		PatternViewport.sizeDelta = new Vector2(0, 71 * patternCount);
		foreach (BMSHeader h in songinfo.Headers)
		{
			GameObject t;
			(t = Instantiate(ButtonPrefab, PatternViewport)).transform.localPosition = new Vector3(300, 30 - 70 * ++i);   //2450
			PatternButtons[i - 1] = t;
			t.GetComponentInChildren<Text>().text = h.Level + " - " + (!string.IsNullOrEmpty(h.Subtitle) ? h.Subtitle : h.Title);
			t.GetComponent<Button>().onClick.AddListener(() =>
			{
				if (!PreviewClips.ContainsKey(songinfo) || Preview.clip != PreviewClips[songinfo])
					Preview.Stop();
				if (PreviewClips.ContainsKey(songinfo))
				{
					Preview.clip = PreviewClips[songinfo];
					if (!Preview.isPlaying)
						Preview.Play();
				}

				if (BMSFileSystem.SelectedHeader == null || string.Compare(BMSFileSystem.SelectedHeader.ParentPath, h.ParentPath) != 0)
					StartCoroutine(LoadBanner(h));
				BMSFileSystem.SelectedHeader = h;
				BMSFileSystem.SelectedPath = h.Path;
				UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##") +
					$" ({(BMSGameManager.Speed * BMSFileSystem.SelectedHeader.Bpm).ToString("0")})");

				if (TitleText.text.CompareTo(songinfo.SongName) != 0)
					TitleText.text = songinfo.SongName;
				SubTitleText.text = (string.IsNullOrEmpty(h.Subtitle)) ? $"[ Level {h.Level} ]" : $"[ {h.Subtitle} ]";
				if (GenreText.text.CompareTo(h.Genre) != 0)
					GenreText.text = $"{h.Artist} / Genre : {h.Genre}";
				BPMText.text = $"BPM {h.Bpm.ToString("0")}";
				InformText.SetActive(true);
				IsReady = true;
			});
		}
	}

	private IEnumerator LoadBanner(BMSHeader h)
	{
		if(string.IsNullOrEmpty(h.BannerPath))
		{
			Banner.texture = null;
			yield break;
		}

		string path = $@"file:\\{h.ParentPath}\{h.BannerPath}";
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
		yield return www.SendWebRequest();
		Texture t = null;
		if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)) t = DownloadHandlerTexture.GetContent(www);
		else if (path.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
		{
			B83.Image.BMP.BMPLoader loader = new B83.Image.BMP.BMPLoader();
			B83.Image.BMP.BMPImage img = loader.LoadBMP(www.downloadHandler.data);
			t = img.ToTexture2D();
		}

		if (t == null) Debug.LogWarning("Error loading banner");
		else
		{
			Banner.texture = t;
			Banner.rectTransform.sizeDelta = new Vector2(300, 80);
		}
	}
}
