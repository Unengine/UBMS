using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KeyConfig
{
	//Only Single supported
	//public int ScratchUp;
	//public int ScratchDown;
	//public int L1;
	//public int L2;
	//public int L3;
	//public int L4;
	//public int L5;
	//public int L6;
	//public int L7;
	public int[] Keys;
	//idx : SUp 0, SDown 1, Lane 2~8

	public KeyConfig() { }

	public KeyConfig(params int[] keys)
	{
		Keys = keys;
	}
}

public class KeySettingManager : MonoBehaviour
{
	public static KeyConfig Config;
	public static string Path;

    void Awake()
    {
        if(Config == null)
		{
			Path = $"{Application.dataPath}/KeyConfig.json";
			LoadKeyConfig();
		}
    }

	public void QuitOptions()
	{
		LoadKeyConfig();
	}

	public void SaveOptions()
	{
		PlayerPrefs.SetInt("LSUp", Config.Keys[0]);
		PlayerPrefs.SetInt("LSDown", Config.Keys[1]);
		PlayerPrefs.SetInt("L1", Config.Keys[2]);
		PlayerPrefs.SetInt("L2", Config.Keys[3]);
		PlayerPrefs.SetInt("L3", Config.Keys[4]);
		PlayerPrefs.SetInt("L4", Config.Keys[5]);
		PlayerPrefs.SetInt("L5", Config.Keys[6]);
		PlayerPrefs.SetInt("L6", Config.Keys[7]);
		PlayerPrefs.SetInt("L7", Config.Keys[8]);
		//Legacy Json
		//File.WriteAllText(Path, JsonMapper.ToJson(Config).ToString());
		LoadKeyConfig();
	}

	public void LoadKeyConfig()
	{
		if (PlayerPrefs.GetInt("KeySet") == 1)
		{
			Config = new KeyConfig(
				PlayerPrefs.GetInt("LSUp"),
				PlayerPrefs.GetInt("LSDown"),
				PlayerPrefs.GetInt("L1"),
				PlayerPrefs.GetInt("L2"),
				PlayerPrefs.GetInt("L3"),
				PlayerPrefs.GetInt("L4"),
				PlayerPrefs.GetInt("L5"),
				PlayerPrefs.GetInt("L6"),
				PlayerPrefs.GetInt("L7")
				);
		}
		else
		{
			PlayerPrefs.SetInt("KeySet", 1);

			PlayerPrefs.SetInt("LSUp", (int)KeyCode.LeftShift);
			PlayerPrefs.SetInt("LSDown", (int)KeyCode.LeftControl);
			PlayerPrefs.SetInt("L1", (int)KeyCode.S);
			PlayerPrefs.SetInt("L2", (int)KeyCode.D);
			PlayerPrefs.SetInt("L3", (int)KeyCode.F);
			PlayerPrefs.SetInt("L4", (int)KeyCode.Space);
			PlayerPrefs.SetInt("L5", (int)KeyCode.J);
			PlayerPrefs.SetInt("L6", (int)KeyCode.K);
			PlayerPrefs.SetInt("L7", (int)KeyCode.L);
			LoadKeyConfig();
		}

		//Legacy Json

		//if (File.Exists(Path))
		//{
		//	Debug.Log("file read");
		//	JsonData jsonConfig = JsonMapper.ToObject(File.ReadAllText(Path));

		//	Config =
		//		new KeyConfig(
		//			(int)jsonConfig["Keys"][0],
		//			(int)jsonConfig["Keys"][1],
		//			(int)jsonConfig["Keys"][2],
		//			(int)jsonConfig["Keys"][3],
		//			(int)jsonConfig["Keys"][4],
		//			(int)jsonConfig["Keys"][5],
		//			(int)jsonConfig["Keys"][6],
		//			(int)jsonConfig["Keys"][7],
		//			(int)jsonConfig["Keys"][8]
		//			);


		//}
		//else
		//{
		//	Debug.Log("file write");
		//	Config =
		//		new KeyConfig(
		//			(int)KeyCode.LeftShift,
		//			(int)KeyCode.LeftControl,
		//			(int)KeyCode.S,
		//			(int)KeyCode.D,
		//			(int)KeyCode.F,
		//			(int)KeyCode.Space,
		//			(int)KeyCode.J,
		//			(int)KeyCode.K,
		//			(int)KeyCode.L
		//			);

		//	File.WriteAllText(Path, JsonMapper.ToJson(Config).ToString());
		//}
	}
}
