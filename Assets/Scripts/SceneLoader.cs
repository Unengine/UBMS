using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {


	public void ChangeScene(int idx)
	{
		StartCoroutine(LoadScene(idx));
	}

	private IEnumerator LoadScene(int idx)
	{
		AsyncOperation op = SceneManager.LoadSceneAsync(idx);

		while(!op.isDone)
		{
			yield return null;
		}
	}
}
