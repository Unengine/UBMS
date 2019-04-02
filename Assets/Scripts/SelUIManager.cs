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
	private GameObject[] PatternButtons;

	// Use this for initialization
	void Start () {
		UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##"));
		Scroll.value = ScrollValue;
	}
	
	// Update is called once per frame
	void Update () {
		ScrollValue = Scroll.value;
		if(Input.GetKeyDown(KeyCode.DownArrow))
		{
			if (BMSGameManager.Speed > 1f)
			{
				BMSGameManager.Speed -= 0.5f;
				UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##"));
			}
		}
		else if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			BMSGameManager.Speed += 0.5f;
			UpdateText(SpeedText, "SPEED " + BMSGameManager.Speed.ToString("#.##"));
		}
	}

	public void UpdateText(Text text, string str) => text.text = str;

	public void DrawSongUI(BMSSongInfo[] songinfos)
	{
		int i = 0;
		foreach(BMSSongInfo s in songinfos)
		{
			GameObject t;
			(t = Instantiate(ButtonPrefab, SongViewport)).transform.localPosition = new Vector3(300, (50 * songinfos.Length) - (100 * ++i) - 2450);   //2450
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
		SongViewport.sizeDelta = new Vector2(0, 100 * songinfos.Length);
	}

	public void DrawPatternUI(BMSSongInfo songinfos, int patternCount)
	{
		int i = 0;
		
		PatternButtons = new GameObject[songinfos.Headers.Count];
		PatternViewport.sizeDelta = new Vector2(0, 100 * patternCount);
		foreach (BMSHeader h in songinfos.Headers)
		{
			GameObject t;
			(t = Instantiate(ButtonPrefab, PatternViewport)).transform.localPosition = new Vector3(300, 50 - 100 * ++i);   //2450
			PatternButtons[i - 1] = t;
			t.GetComponentInChildren<Text>().text = h.Level + " - " + (!string.IsNullOrEmpty(h.Subtitle) ? h.Subtitle : h.Title);
			t.GetComponent<Button>().onClick.AddListener(() =>
			{
				BMSFileSystem.SelectedHeader = h;
				BMSFileSystem.SelectedPath = h.ParentPath;
				UnityEngine.SceneManagement.SceneManager.LoadScene(0);
			});
		}
	}

}
