using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using B83.Image.BMP;

public class GameUIManager : MonoBehaviour
{

	public bool IsPrepared { get; set; } = false;
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
	private RawImage Bga;
	[SerializeField]
	private Text ComboText;

	private Animator ComboAnim;

	// Use this for initialization
	private void Awake ()
	{
		BGImageTable = new Dictionary<string, string>();
		BGSprites = new Dictionary<string, Texture2D>();
		ComboAnim = ComboText.GetComponent<Animator>();
	}

	public void LoadImages()
	{
		StartCoroutine(CLoadImages());
	}

	private IEnumerator CLoadImages()
	{
		foreach (KeyValuePair<string, string> p in BGImageTable)
		{
			string path = "file://" + Directory.GetParent(BMSFileSystem.SelectedPath) + "/" + p.Value;
			WWW www = new WWW(path);

			yield return www;

			Texture2D t = null;
			if (www.url.EndsWith("bmp", System.StringComparison.OrdinalIgnoreCase))
			{
				BMPLoader loader = new BMPLoader();
				BMPImage img = loader.LoadBMP(www.bytes);
				t = img.ToTexture2D();
			}
			else if (www.url.EndsWith("png", System.StringComparison.OrdinalIgnoreCase))
			{
				t = www.texture;
			}

			BGSprites.Add(p.Key, t);

		}
		IsPrepared = true;
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
			Bga.texture = BGSprites[key];
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
