using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMSParser : MonoBehaviour {

    private static BMSParser Inst;
    public static BMSParser Instance
    {
        get
        {
            if (!Inst) Inst = FindObjectOfType<BMSParser>();
            if(!Inst)
            {
                Debug.LogError("There should be one GameObject with BMSParser!");
                return null;
            }
			
            return Inst;
        }
    }

	//public SpriteRenderer bg;
	public BMSPattern Pat { get; private set; }
	public BMSHeader Header { get; private set; }
    public SoundManager Sm;
	public GameUIManager GameUI;
    private string[] BmsFile { get; set; }
    private Dictionary<string, double> ExBpms;
	private List<string> KeySoundPathes;

    private void Awake()
    {
		KeySoundPathes = new List<string>()
		{
			Capacity = 1000
		};
		ExBpms = new Dictionary<string, double>();
        Sm = GetComponent<SoundManager>();
    }

    public void Parse()
    {
		Debug.Log("parse");
		//Header = new BMSHeader();
		Pat = new BMSPattern();
		GetFile();
		ParseGameHeader();
		ParseMainData();
	}

	public void GetFile()
	{
		Header = BMSGameManager.Header;
		BmsFile = System.IO.File.ReadAllLines(BMSGameManager.Header.Path);
	}

    private void ParseGameHeader()
    {
		foreach (string s in BmsFile)
		{
			if (s.Length <= 3) continue;

			if (s.Length >= 4 && string.Compare(s.Substring(0, 4), "#WAV", true) == 0)
			{
				int key = Decode36(s.Substring(4, 2));
				string path = s.Substring(7, s.Length - 11);
				Sm.Pathes.Add(key, path);
			}
			else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#LNOBJ", true) == 0)
			{
				Header.Lnobj = Decode36(s.Substring(7, 2));
				Header.LnType |= Lntype.LNOBJ;
			}
			else if (s.Length >= 6 && string.Compare(s.Substring(0, 4), "#BMP", true) == 0)
			{
				string key = s.Substring(4, 2);
				string extend = s.Substring(s.Length - 3, 3);
				string Path = s.Substring(7, s.Length - 7);
				if (string.Compare(extend, "mpg", true) == 0)
				{
					Pat.BGVideoTable.Add(key, Path);
				}
				else if (string.Compare(extend, "bmp", true) == 0)
				{
					GameUI.BGImageTable.Add(key, Path);
				}
				else if (string.Compare(extend, "png", true) == 0)
				{
					GameUI.BGImageTable.Add(key, Path);
				}
			}
			else if (s.Length >= 6 && string.Compare(s.Substring(0, 4), "#BPM", true) == 0)
			{
				if (s[4] == ' ')
					Header.Bpm = double.Parse(s.Substring(5));
				else
				{
					string key = s.Substring(4, 2);
					double bpm = double.Parse(s.Substring(7));
					//Debug.Log(exBpms.Count + "/" + bpm);
					ExBpms.Add(key, bpm);
				}
			}
			else if (s.Length >= 7 && string.Compare(s.Substring(0, 5), "#STOP", true) == 0)
			{
				if (s[7] == ' ')
				{
					string sub = s.Substring(5, 2);
					double stopDuration = int.Parse(s.Substring(8)) / 192.0;
					//pat.LegacyStopDuratns.Add(stopDuration); // 나누기 192
					if (!Pat.StopDurations.ContainsKey(sub))
					{
						Pat.StopDurations.Add(sub, stopDuration);
					}
				}
			}
			else if (s.Length >= 30 && s.CompareTo("*---------------------- MAIN DATA FIELD") == 0) break;

		}
    }

    private void ParseMainData()
    {
		bool ifBlockOpen = false;
        bool isIfVaild = false;
		double beatC = 1.0f;
		int RandomValue = 1;

		int LNBits = 0;
		System.Random rand = new System.Random();

        foreach(string s in BmsFile)
        {
            if (s.Length == 0) continue;
            if (s[0] != '#') continue;

			if (s.Length >= 9 && string.Compare(s.Substring(0, 7), "#random", true) == 0)
			{
				//RandomValue = Random.Range(1, int.Parse(s.Substring(8)) + 1);
				RandomValue = rand.Next(1, int.Parse(s.Substring(8)) + 1);
				continue;
			}

			if (s.Length >= 3 && string.Compare(s.Substring(0, 3), "#if", true) == 0)
            {
                ifBlockOpen = true;
                if (s[4] - '0' == RandomValue)
                {
                    isIfVaild = true;
                }
                continue;
            }

            if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#endif", true) == 0)
            {
				ifBlockOpen = false;
                isIfVaild = false;
                continue;
            }
			if (ifBlockOpen && !isIfVaild) continue;

            int bar;

            if (!int.TryParse(s.Substring(1, 3), out bar)) continue;

            if (Pat.BarCount < bar) Pat.BarCount = bar; //나중에 1 더해야함

            if (s[4] == '1' || s[4] == '5')
            {
                int line, beatLength;
                line = s[5] - '1';
                beatLength = (s.Length - 7) / 2;

                for (int i = 7; i < s.Length - 1; i += 2)
                {
                    int keySound = Decode36(s.Substring(i, 2));
                    if (keySound != 0)
                    {
						if(s[4] == '5')
						{
							if((LNBits & (1 << line)) != 0)
							{
								Pat.AddNote(line, bar, (i - 7) / 2, beatLength, -1, 1);
								LNBits &= ~(1 << line);	//erase bit
								continue;
							}
							else LNBits |= (1 << line);	//write bit
						}
						if (Header.LnType.HasFlag(Lntype.LNOBJ) && keySound == Header.Lnobj)
							Pat.AddNote(line, bar, (i - 7) / 2, beatLength, keySound, 1);
						else
							Pat.AddNote(line, bar, (i - 7) / 2, beatLength, keySound, 0);
					}
                }
            }
            else if (s[4] == '0')
            {
                int beatLength;
                if (s[5] == '1')
                {
                    beatLength = (s.Length - 7) / 2;
                    //bar = int.Parse(s.Substring(1, 3));
                    for (int i = 7; i < s.Length - 1; i += 2)
                    {
                        int keySound = Decode36(s.Substring(i, 2));

                        if (keySound != 0)
                        {
                            Pat.AddBGSound(bar, (i - 7) / 2, beatLength, keySound);
                        }
                    }
                }
                else if (s[5] == '2')
                {
                    beatC = double.Parse(s.Substring(7));
                    Pat.AddNewBeatC(bar, beatC);
                }
                else if (s[5] == '3')
                {
                    beatLength = (s.Length - 7) / 2;
                    for (int i = 7; i < s.Length - 1; i += 2)
                    {
						double bpm = int.Parse(s.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);

                        if (bpm != 0) Pat.AddBPM(bar, (i - 7) / 2, beatLength, bpm);
                    }
                }
                else if (s[5] == '4')
                {
                    beatLength = (s.Length - 7) / 2;
					for (int i = 7; i < s.Length - 1; i += 2)
					{
						string key = s.Substring(i, 2);

						if (string.Compare(key, "00") != 0)
							if (Pat.BGVideoTable.ContainsKey(key))
							{
								Pat.AddBGAChange(bar, (i - 7) / 2, beatLength, key);
							}
							else
							{
								Pat.AddBGAChange(bar, (i - 7) / 2, beatLength, key, true);
							}
					}
                }
                else if (s[5] == '8')
                {
                    beatLength = (s.Length - 7) / 2;
                    //int idx = Decode36(s.Substring(7, 2)) - 1;
                    for (int i = 7; i < s.Length - 1; i += 2)
                    {
                        string key = s.Substring(i, 2);
						if (key.CompareTo("00") != 0) Pat.AddBPM(bar, (i - 7) / 2, beatLength, ExBpms[key]);
                    }
                }
                else if (s[5] == '9')
                {
                    beatLength = (s.Length - 7) / 2;
                    for (int i = 7; i < s.Length - 1; i += 2)
                    {
						string sub = s.Substring(i, 2);
						if (string.Compare(sub, "00") != 0) Pat.AddStop(bar, (i - 7) / 2, beatLength, sub);
                    }
                }
            }
			else if (s[4] == 'D' || s[4] == 'E')
			{
				int line, beatLength;
				line = s[5] - '1';
				beatLength = (s.Length - 7) / 2;

				for (int i = 7; i < s.Length - 1; i += 2)
				{
					int keySound = Decode36(s.Substring(i, 2));
					if (keySound != 0)
					{
						Pat.AddNote(line, bar, (i - 7) / 2, beatLength, keySound, -1);
					}
				}
			}

        }
    }

    public static int Decode36(string str)
    {
        if (str.Length != 2) return -1;

        int result = 0;
        if (str[1] >= 'A')
            result += str[1] - 'A' + 10;
        else
            result += str[1] - '0';
        if (str[0] >= 'A')
            result += (str[0] - 'A' + 10) * 36;
        else
            result += (str[0] - '0') * 36;

        return result;
    }
}