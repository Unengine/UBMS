using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelUIManager : MonoBehaviour {

	public static float ScrollValue = 1;
	public Scrollbar Scroll;
	public Text SpeedText;

	[SerializeField]
	private GameObject ButtonPrefab;
	[SerializeField]
	private RectTransform SongViewport;
	[SerializeField]
	private RectTransform PatternViewport;
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

	private GameObject[] PatternButtons;
	private bool IsReady = false;

	// Use this for initialization
	void Start () {
		UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##"));
		Scroll.value = ScrollValue;
	}
	
	// Update is called once per frame
	void Update () {
		ScrollValue = Scroll.value;
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (BMSGameManager.Speed > 1f)
			{
				BMSGameManager.Speed -= 0.5f;
				UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##"));
			}
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			BMSGameManager.Speed += 0.5f;
			UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##"));
		}
		else if (IsReady && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene(0);
		}
	}

	public void UpdateText(Text text, string str) => text.text = str;

	public void DrawSongUI(BMSSongInfo[] songinfos)
	{
		int i = 0;
		foreach(BMSSongInfo s in songinfos)
		{
			GameObject t;
			(t = Instantiate(ButtonPrefab, SongViewport)).transform.localPosition = new Vector3(300, (50 * songinfos.Length) - (70 * ++i) - 2770);   //2450
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
		SongViewport.sizeDelta = new Vector2(0, 70 * songinfos.Length);
	}

	public void DrawPatternUI(BMSSongInfo songinfos, int patternCount)
	{
		int i = 0;
		
		PatternButtons = new GameObject[songinfos.Headers.Count];
		PatternViewport.sizeDelta = new Vector2(0, 70 * patternCount);
		foreach (BMSHeader h in songinfos.Headers)
		{
			GameObject t;
			(t = Instantiate(ButtonPrefab, PatternViewport)).transform.localPosition = new Vector3(300, 30 - 70 * ++i);   //2450
			PatternButtons[i - 1] = t;
			t.GetComponentInChildren<Text>().text = h.Level + " - " + (!string.IsNullOrEmpty(h.Subtitle) ? h.Subtitle : h.Title);
			t.GetComponent<Button>().onClick.AddListener(() =>
			{
				BMSFileSystem.SelectedHeader = h;
				BMSFileSystem.SelectedPath = h.ParentPath;

				if (TitleText.text.CompareTo(songinfos.SongName) != 0)
					TitleText.text = songinfos.SongName;
				SubTitleText.text = (string.IsNullOrEmpty(h.Subtitle)) ? $"[ Level {h.Level} ]" : $"[ {h.Subtitle} ]";
				if (GenreText.text.CompareTo(h.Genre) != 0)
					GenreText.text = $"{h.Artist} / Genre : {h.Genre}";
				BPMText.text = $"BPM {h.Bpm.ToString("0")}";
				InformText.SetActive(true);
				IsReady = true;
			});
		}
	}

}
