using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;

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
		//ScratchUp = (int)su;
		//ScratchDown = (int)sd;
		//L1 = (int)l1;
		//L2 = (int)l2;
		//L3 = (int)l3;
		//L4 = (int)l4;
		//L5 = (int)l5;
		//L6 = (int)l6;
		//L7 = (int)l7;
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
		File.WriteAllText(Path, JsonMapper.ToJson(Config).ToString());
		LoadKeyConfig();
	}

	public void LoadKeyConfig()
	{
		if (File.Exists(Path))
		{
			Debug.Log("file read");
			JsonData jsonConfig = JsonMapper.ToObject(File.ReadAllText(Path));

			Config =
				new KeyConfig(
					(int)jsonConfig["Keys"][0],
					(int)jsonConfig["Keys"][1],
					(int)jsonConfig["Keys"][2],
					(int)jsonConfig["Keys"][3],
					(int)jsonConfig["Keys"][4],
					(int)jsonConfig["Keys"][5],
					(int)jsonConfig["Keys"][6],
					(int)jsonConfig["Keys"][7],
					(int)jsonConfig["Keys"][8]
					);


		}
		else
		{
			Debug.Log("file write");
			Config =
				new KeyConfig(
					(int)KeyCode.LeftShift,
					(int)KeyCode.LeftControl,
					(int)KeyCode.S,
					(int)KeyCode.D,
					(int)KeyCode.F,
					(int)KeyCode.Space,
					(int)KeyCode.J,
					(int)KeyCode.K,
					(int)KeyCode.L
					);

			File.WriteAllText(Path, JsonMapper.ToJson(Config).ToString());
		}
	}
}
