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

	// Use this for initialization
	private void Awake ()
	{
		Debug.Log("awake");
		BGImageTable = new Dictionary<string, string>();
		BGSprites = new Dictionary<string, Texture2D>();
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
}
