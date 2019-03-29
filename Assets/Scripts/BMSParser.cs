using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class BMSParser : MonoBehaviour {

    private static BMSParser inst;
    public static BMSParser instance
    {
        get
        {
            if (!inst) inst = FindObjectOfType<BMSParser>();
            if(!inst)
            {
                Debug.LogError("There should be one GameObject with BMSParser!");
                return null;
            }
            return inst;
        }
    }

    public BMSDrawer drawer;
	//public SpriteRenderer bg;
    public SoundManager sm;
    public BMSHeader header { get; private set; }
    public BMSPattern pat { get; private set; }
    private string path;
    private string[] bmsFile { get; set; }
    private List<double> exBpms;

    private void Awake()
    {
        exBpms = new List<double>()
        {
            Capacity = 3
        };
        sm = GetComponent<SoundManager>();
        path =
		//"BMSFiles/CrystalWorld/"
		//"BMSFiles/EndTime/"
		//"BMSFiles/JackTheRipper/"
		//"BMSFiles/Halcyon/"
		//"BMSFiles/YumeLyrith/"
		//"BMSFiles/EOS_master0906/"
		//"BMSFiles/Doppelganger_LeaF/"
		//"BMSFiles/Aleph0/"
		//"BMSFiles/DeadSoul/"
		//"BMSFiles/Lots of Spices/"
		"BMSFiles/Engine/"
        //"BMSFiles/3rd Avenue/"
		;
        Init();
        GetFile(Resources.Load<TextAsset>(path +
			//"_crystal-world_r_7a"
			//"EndTimeTB"	
			//"jacktheripper_29a"
			//"_hal_A"
			//"_7ANOTHER"
			//"eos_m"
			//"_A7"
			//"_7ANOTHER"
			//"soundsouler_deadsoul_Revive"
	        //"778_LOSmineds"
			"engine_XYZ"
            //"3AE7_XYZ"
			));
            
        ParseHeader();
        PrintHeader();
        sm.AddAudioClips(path, pat.KeySounds);
        ParseMainData();
        pat.CalCulateBeats();
        Debug.Log("Done parsing");
        //Debug.Log(path + header.BGImagePath);
        //bg.sprite = Resources.Load<Sprite>(path + header.BGImagePath);
        //bg.color = new Color(1, 1, 1, 0.5f);
        
    }

    public void Init()
    {
        header = new BMSHeader();
        pat = new BMSPattern();
    }

    public void GetFile(TextAsset text)
    {
        char[] delim = new char[] { '\n', '\r' };
        bmsFile = text.text.Split(delim);
    }

    public void ParseHeader()
    {
        int prevKeySoundIdx = 0;
        foreach (string s in bmsFile)
        {
            if (s.Length <= 3) continue;

            if (s.Length > 10 && string.Compare(s.Substring(0, 10), "#PLAYLEVEL") == 0)
                header.Level = int.Parse(s.Substring(11));
            else if (s.Length > 11 && string.Compare(s.Substring(0, 10), "#STAGEFILE") == 0)
            {
                //Debug.Log(s);
                header.BGImagePath = s.Substring(11, s.Length - 15);
            }
            else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#PLAYER") == 0) header.Player = s[8] - '0';
            else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#ARTIST") == 0) header.Artist = s.Substring(8, s.Length - 8);
            else if (s.Length >= 7 && string.Compare(s.Substring(0, 7), "#LNTYPE") == 0) header.LnType |= (BMSHeader.Lntype)(1 << (s[8] - '0'));
            else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#LNOBJ") == 0)
            {
                header.Lnobj = Decode36(s.Substring(7, 2));
                header.LnType |= BMSHeader.Lntype.LNOBJ;
            }
            else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#GENRE") == 0) header.Genre = s.Substring(7);
            else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#TITLE") == 0) header.Title = s.Substring(7);
            else if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#TOTAL") == 0) header.Total = double.Parse(s.Substring(7));
            else if (s.Length >= 5 && string.Compare(s.Substring(0, 5), "#RANK") == 0) header.Rank = int.Parse(s.Substring(6));
            else if (s.Length >= 4 && string.Compare(s.Substring(0, 4), "#WAV") == 0)
            {
                int idx = Decode36(s.Substring(4, 2));
                if (idx == prevKeySoundIdx + 1)
                {
                    pat.KeySounds.Add(s.Substring(7, s.Length - 11));
                    ++prevKeySoundIdx;
                }
                else
                {
                    for (int i = prevKeySoundIdx + 1; i < idx; ++i)
                    {
                        pat.KeySounds.Add(string.Empty);

                    }
                    pat.KeySounds.Add(s.Substring(7, s.Length - 11));
                    prevKeySoundIdx = idx;
                }
            }
            else if (s.Length >= 6 && string.Compare(s.Substring(0, 4), "#BPM") == 0)
            {
                if (s[4] == ' ')
                    header.Bpm = double.Parse(s.Substring(5));
                else
                {
					double bpm = double.Parse(s.Substring(7));
                    //Debug.Log(exBpms.Count + "/" + bpm);
                    exBpms.Add(bpm);
                }
            }
            else if (s.Length >= 7 && string.Compare(s.Substring(0, 5), "#STOP") == 0)
            {
                if (s[7] == ' ')
                {
					string sub = s.Substring(5, 2);
                    double stopDuration = int.Parse(s.Substring(8)) / 192.0;
                    pat.LegacyStopDurations.Add(stopDuration); // 나누기 192
					if (!pat.StopDurations.ContainsKey(sub))
					{
						pat.StopDurations.Add(sub, stopDuration);
					}
                }
            }
            else if(s.Length >= 9 && string.Compare(s.Substring(0, 7), "#random") == 0)
            {
                header.RandomValue = Random.Range(1, int.Parse(s.Substring(8)) + 1);
            }
        }
    }

    public void ParseMainData()
    {
        bool ifBlockOpen = false;
        bool isIfVaild = false;
		double beatC = 1.0f;

        foreach(string s in bmsFile)
        {
            if (s.Length == 0) continue;
            if (s[0] != '#') continue;

            if (ifBlockOpen && !isIfVaild)
            {
                continue;
            }

            if (s.Length >= 3 && string.Compare(s.Substring(0, 3), "#if") == 0)
            {
                //Debug.Log("if");
                ifBlockOpen = true;
                if (s[4] - '0' == header.RandomValue)
                {
                    isIfVaild = true;
                }
                continue;
            }

            if (s.Length >= 6 && string.Compare(s.Substring(0, 6), "#endif") == 0)
            {
                ifBlockOpen = false;
                isIfVaild = false;
                continue;
            }

            int bar;

            if (!int.TryParse(s.Substring(1, 3), out bar)) continue;

            if (pat.BarCount < bar) pat.BarCount = bar; //나중에 1 더해야함

            if (s[4] == '1')
            {
                int line, beatLength;
                line = s[5] - '1';
                beatLength = (s.Length - 7) / 2;

                for (int i = 7; i < s.Length - 1; i += 2)
                {
                    int keySound = Decode36(s.Substring(i, 2));
                    if (keySound != 0)
                    {
                        if (header.LnType.HasFlag(BMSHeader.Lntype.LNOBJ) && keySound == header.Lnobj)
                            pat.AddNote(line, bar, (i - 7) / 2, beatLength, keySound, 1);
                        else
                            pat.AddNote(line, bar, (i - 7) / 2, beatLength, keySound, 0);
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
                            pat.AddBGSound(bar, (i - 7) / 2, beatLength, keySound);
                        }
                    }
                }
                else if (s[5] == '2')
                {
                    beatC = double.Parse(s.Substring(7));
                    pat.AddNewBeatC(bar, beatC);
                }
                else if (s[5] == '3')
                {
                    beatLength = (s.Length - 7) / 2;
                    for (int i = 7; i < s.Length - 1; i += 2)
                    {
						double bpm = int.Parse(s.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);

                        if (bpm != 0) pat.AddBPM(bar, (i - 7) / 2, beatLength, bpm);
                    }
                }
                else if (s[5] == '4')
                {
                    beatLength = (s.Length - 7) / 2;
                    for (int i = 7; i < s.Length - 1; i += 2)
                    {
                        int idx = Decode36(s.Substring(i, 2));
                        if (idx != 0) pat.AddBGAChange(bar, (i - 7) / 2, beatLength, idx);
                    }
                }
                else if (s[5] == '8')
                {
                    beatLength = (s.Length - 7) / 2;
                    //int idx = Decode36(s.Substring(7, 2)) - 1;
                    for (int i = 7; i < s.Length - 1; i += 2)
                    {
                        int idx = int.Parse(s.Substring(i, 2), System.Globalization.NumberStyles.HexNumber) - 1;
                        if (idx >= 0) pat.AddBPM(bar, (i - 7) / 2, beatLength, exBpms[idx]);
                    }
                }
                else if (s[5] == '9')
                {
                    beatLength = (s.Length - 7) / 2;
                    for (int i = 7; i < s.Length - 1; i += 2)
                    {
						string sub = s.Substring(i, 2);
						if (string.Compare(sub, "00") != 0) pat.AddStop(bar, (i - 7) / 2, beatLength, sub);
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
						pat.AddNote(line, bar, (i - 7) / 2, beatLength, keySound, -1);
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

    public void PrintHeader()
    {
        foreach (FieldInfo f in header.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            //Debug.Log(f.GetValue(header));
        }

        //foreach (string s in header.KeySounds) Debug.Log(s);
    }
}