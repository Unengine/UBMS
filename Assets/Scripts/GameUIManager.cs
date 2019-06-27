using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using B83.Image.BMP;
using UnityEngine.Networking;

public class GameUIManager : MonoBehaviour
{
	public float YRatio { get; set; } = 1;
	public bool IsPrepared { get; set; } = false;
	public RawImage Bga;
	public Dictionary<string, string> BGImageTable { get; set; }
	public Dictionary<string, Texture2D> BGSprites { get; set; }

	[SerializeField]
	private Slider HPBar;
	[SerializeField]
	private Text BPMText;
	[SerializeField]
	private Text ScoreText;
	[SerializeField]
	private Text StatisticsText;
	[SerializeField]
	private Text FSText;
	[SerializeField]
	private Text ComboText;
	[SerializeField]
	private Image StartPanel;
	[SerializeField]
	private Sprite GrooveSprite;
	[SerializeField]
	private Sprite SurvSprite;
	[SerializeField]
	private Sprite ExSurvSprite;
	[SerializeField]
	private RawImage BGAVideoImage;


	private Animator ComboAnim;

	// Use this for initialization
	private void Awake ()
	{
		BGImageTable = new Dictionary<string, string>();
		BGSprites = new Dictionary<string, Texture2D>();
		ComboAnim = ComboText.GetComponent<Animator>();
	}

	public void SetVideoRatio()
	{
		BGAVideoImage.transform.localScale = new Vector3(1, YRatio, 1);
		if (YRatio != 1.0f)
		{
			YRatio = 1.0f;
		}
	}

	public void SetHPBarSprite(GaugeType type)
	{
		Sprite sprite;

		if (type <= GaugeType.Groove) sprite = GrooveSprite;
		else if (type <= GaugeType.Survival) sprite = SurvSprite;
		else sprite = ExSurvSprite;
		GameObject.Find("Fill").GetComponent<Image>().sprite = sprite;
	}

	public void LoadBackBmp()
	{
		if (!string.IsNullOrEmpty(BMSGameManager.Header.BackbmpPath))
			StartCoroutine(CLoadBackBmp());
	}

	private IEnumerator CLoadBackBmp()
	{
		string path = "file://" + BMSGameManager.Header.ParentPath + "/" + BMSGameManager.Header.BackbmpPath;

		Texture2D t = null;
		if (path.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
		{
			UnityWebRequest www = UnityWebRequest.Get(path);
			yield return www.SendWebRequest();

			BMPLoader loader = new BMPLoader();
			BMPImage img = loader.LoadBMP(www.downloadHandler.data);
			t = img.ToTexture2D();
		}
		else if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
		{
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
			yield return www.SendWebRequest();

			t = (www.downloadHandler as DownloadHandlerTexture).texture;
		}

		//Bga.sprite = Sprite.Create(t, new Rect(0.0f, 0.0f, t.width, t.height), new Vector2(0.5f, 0.5f), 100.0f);
		Bga.texture = t;
		float timer = 0;
		yield return new WaitUntil(() =>
		{
			if (timer < 0.4f)
			{
				timer += Time.deltaTime;
				Bga.color = new Color(1, 1, 1, timer * 2.5f);
				return false;
			}
			else return true;
		});
		Bga.color = Color.white;
	}

	public void LoadImages()
	{
		StartCoroutine(CLoadImages());
	}

	private IEnumerator CLoadImages()
	{
		foreach (KeyValuePair<string, string> p in BGImageTable)
		{
			string path = "file://" + BMSGameManager.Header.ParentPath + "/" + p.Value;


			Texture2D t = null;
			if (path.EndsWith(".bmp", System.StringComparison.OrdinalIgnoreCase))
			{
				UnityWebRequest www = UnityWebRequest.Get(path);
				yield return www.SendWebRequest();

				BMPLoader loader = new BMPLoader();
				BMPImage img = loader.LoadBMP(www.downloadHandler.data);
				t = img.ToTexture2D();
			}
			else if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
			{
				UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
				yield return www.SendWebRequest();

				t = (www.downloadHandler as DownloadHandlerTexture).texture;
			}
			else if (path.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
			{
				UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
				yield return www.SendWebRequest();
				t = (www.downloadHandler as DownloadHandlerTexture).texture;
			}
			BGSprites.Add(p.Key, t);

		}
		IsPrepared = true;
	}

	public void UpdateComboText(string str)
	{
		ComboText.text = str;
	}

	public void ComboUpTxt(JudgeType judge, int combo)
	{
		ComboAnim.Rebind();
		ComboAnim.Play("ComboUp");
		ComboText.text = judge + " " + combo;
	}

	public void ComboUpTxt(string str)
	{
		ComboAnim.Rebind();
		ComboAnim.Play("ComboUp");
		ComboText.text = str;
	}

	public void ChangeBGA(string key)
	{
		if (BGSprites.ContainsKey(key))
		{
			Bga.texture = BGSprites[key];
			//Texture2D t = BGSprites[key];
			//Bga.sprite = Sprite.Create(t, new Rect(0.0f, 0.0f, t.width, t.height), new Vector2(0.5f, 0.5f), 100.0f);
		}
	}

	public void UpdateFSText(double diff)
	{
		if (Utility.DAbs(diff) <= 21.0)
		{
			FSText.text = string.Empty;
			return;
		}

		FSText.text = ((diff > 0) ? "FAST +" : "SLOW -") + Utility.DCeilToInt(Utility.DAbs(diff)) + "ms";
	}

	public void UpdateScore(BMSResult res, float hp, double accuracy)
	{
		HPBar.value = hp;

		ScoreText.text =
			((int)(hp * 100)).ToString() + " %\n"
			+ "\nSCORE : " + res.Score.ToString("D4")
			+ "\nACCURACY : " + accuracy.ToString("P");

		StatisticsText.text =
			$"PGREAT : {res.Pgr.ToString("D4")}\n" +
			$"GREAT : {res.Gr.ToString("D4")}\n" +
			$"GOOD : {res.Good.ToString("D4")}\n" +
			$"BAD : {res.Bad.ToString("D4")}\n" +
			$"POOR : {res.Poor.ToString("D4")}";
	}

	public void UpdateBPMText(double bpm)
	{
		if (bpm >= 1000 || bpm < 0) bpm = 0;
		BPMText.text = "BPM\n" + ((int)bpm).ToString("D3");
	}
}
