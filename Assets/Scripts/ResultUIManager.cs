﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		Statistics.text =
			$"PGREAT : {BMSGameManager.Res.Pgr.ToString("D4")}\n" +
			$"GREAT : {BMSGameManager.Res.Gr.ToString("D4")}\n" +
			$"GOOD : {BMSGameManager.Res.Good.ToString("D4")}\n" +
			$"BAD : {BMSGameManager.Res.Bad.ToString("D4")}\n" +
			$"POOR : {BMSGameManager.Res.Poor.ToString("D4")}\n\n" +
			$"SCORE : {BMSGameManager.Res.Score.ToString("D4")}\n" +
			$"ACCAURACY : {BMSGameManager.Res.Accaurcy.ToString("P")}";

		int tot = (BMSGameManager.Res.Pgr + BMSGameManager.Res.Gr + BMSGameManager.Res.Good +
			BMSGameManager.Res.Bad + BMSGameManager.Res.Poor) * 2;

		double ratio = BMSGameManager.Res.Score / (double)tot;

		if (ratio >= 1) Rank.text = "WTF";
		else if (ratio >= 0.9) Rank.text = "AAA";
		else if (ratio >= 0.8) Rank.text = "AA";
		else if (ratio >= 0.7) Rank.text = "A";
		else if (ratio >= 0.6) Rank.text = "B";
		else if (ratio >= 0.5) Rank.text = "C";
		else if (ratio >= 0.4) Rank.text = "D";
		else if (ratio >= 0.3) Rank.text = "E";
		else Rank.text = "F";
	}
}
