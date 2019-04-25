using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class ResultUIManager : MonoBehaviour
{
	[SerializeField]
	private Text Statistics;
	[SerializeField]
	private Text Rank;

	public void Awake()
	{
		DrawResults();
	}

	private void DrawResults()
	{
		string clearStr = (BMSGameManager.Res.ClearGauge == -1) ? "FAILED" :
			$"{(GaugeType)BMSGameManager.Res.ClearGauge} Cleared"
			;

		Statistics.text =
			$"NOTECOUNT : {BMSGameManager.Res.NoteCount.ToString("D4")}\n" +
			$"PGREAT : {BMSGameManager.Res.Pgr.ToString("D4")}\n" +
			$"GREAT : {BMSGameManager.Res.Gr.ToString("D4")}\n" +
			$"GOOD : {BMSGameManager.Res.Good.ToString("D4")}\n" +
			$"BAD : {BMSGameManager.Res.Bad.ToString("D4")}\n" +
			$"POOR : {BMSGameManager.Res.Poor.ToString("D4")}\n\n" +
			$"SCORE : {BMSGameManager.Res.Score.ToString("D4")}\n" +
			$"ACCURACY : {BMSGameManager.Res.Accuracy.ToString("P")}\n" +
			clearStr
			;


		double ratio = BMSGameManager.Res.Score / (double)(BMSGameManager.Res.NoteCount * 2);

		if (ratio >= 1) Rank.text = "WTF";
		else if (ratio >= 0.9) Rank.text = "AAA";
		else if (ratio >= 0.8) Rank.text = "AA";
		else if (ratio >= 0.7) Rank.text = "A";
		else if (ratio >= 0.6) Rank.text = "B";
		else if (ratio >= 0.5) Rank.text = "C";
		else if (ratio >= 0.4) Rank.text = "D";
		else if (ratio >= 0.3) Rank.text = "E";
		else Rank.text = "F";

		if(BMSGameManager.WillSaveData)
		{
			BMSHeader header = BMSGameManager.Header;
			string name = header.Path.Substring(header.Path.IndexOf("BMSFiles") + 9).Replace('\\', '_');
			string path = $"{Application.dataPath}/{name}.Result.json";

			if (File.Exists(path))
			{
				JsonData prevResJson = JsonMapper.ToObject(File.ReadAllText(path));

				if ((int)prevResJson["Score"] > BMSGameManager.Res.Score) return;

				else if ((int)prevResJson["Score"] == BMSGameManager.Res.Score)
					if ((double)prevResJson["Accuracy"] >= BMSGameManager.Res.Accuracy)
						return;
			}

			JsonData resJson = JsonMapper.ToJson(BMSGameManager.Res);
			File.WriteAllText(path, resJson.ToString());
		}
	}
}
