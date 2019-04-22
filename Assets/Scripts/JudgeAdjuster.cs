using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JudgeAdjuster : MonoBehaviour, IPointerClickHandler
{
	private Text text;

	private void Start()
	{
		text = GetComponentInChildren<Text>();
		UpdateText();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (BMSGameManager.JudgeAdjValue > 29) return;
			BMSGameManager.JudgeAdjValue += 1;
			UpdateText();
		}
		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			if (BMSGameManager.JudgeAdjValue < -29) return;
			BMSGameManager.JudgeAdjValue -= 1;
			UpdateText();
		}
	}

	private void UpdateText()
	{
		text.text = BMSGameManager.JudgeAdjValue.ToString() + "ms";
	}
}
