using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGAChange : BMSObject
{
    public int Idx { get; set; } = 0;
    public bool isPic;
    public BGAChange(int bar, int idx, double beat, double beatLength, bool isPic) : base(bar, beat, beatLength)
    {
        Idx = idx;
    }
}

public class Line
{
    public List<Note> noteList;
    public List<Note> landMineList;
    public Line()
    {
        noteList = new List<Note>()
        {
            Capacity = 225
        };
        landMineList = new List<Note>()
        {
            Capacity = 20
        };
    }
}

public abstract class BMSObject : IComparable<BMSObject>
{
    public int Bar { get; protected set; }
    public double Beat { get; protected set; }
    public double Timing { get; set; }

    public BMSObject(int bar, double beat, double beatLength)
    {
        Bar = bar;
        Beat = (beat / beatLength) * 4.0;
    }

    public void CalculateBeat(double prevBeats, double beatC)
    {
        Beat = Beat * beatC + prevBeats;
    }

    public int CompareTo(BMSObject other)
    {
        if (Beat < other.Beat) return 1;
        if (Beat == other.Beat) return 0;
        return -1;
    }
}

public class Note : BMSObject
{
    public int KeySound { get; private set; }
    public int Extra { get; set; }
    public GameObject Model { get; set; }

    public Note(int bar, int keySound, double beat, double beatLength, int extra) : base(bar, beat, beatLength)
    {
        KeySound = keySound;
        Extra = extra;
    }
}

public class BPM : BMSObject
{
    public double Bpm { get; private set; }

    public BPM(int bar, double bpm, double beat, double beatLength) : base(bar, beat, beatLength)
    {
        Bpm = bpm;
    }
}

public class Stop : BMSObject
{
	public string Key;
    public Stop(int bar, string key, double beat, double beatLength) : base(bar, beat, beatLength)
    {
		Key = key;
    }
}

public class BMSPattern {

    public int BarCount { get; set; } = 0;
    public List<BGAChange> BGAChanges { get; set; }
    public List<Note> BGSounds { get; set; }
    public List<string> KeySounds { get; set; }
    public List<BPM> Bpms { get; set; }
    public List<Stop> Stops { set; get; }
    public List<double> LegacyStopDurations { get; set; }
	public Dictionary<string, double> StopDurations { get; set; }
    public Dictionary<int, double> BeatCTable { get; set; }
    public Line[] Lines { get; set; }

    public BMSPattern()
    {
        BeatCTable = new Dictionary<int, double>();
		StopDurations = new Dictionary<string, double>();
        LegacyStopDurations = new List<double>()
        {
            Capacity = 5
        };
        Stops = new List<Stop>()
        {
            Capacity = 5
        };
        Bpms = new List<BPM>()
        {
            Capacity = 5
        };
        KeySounds = new List<string>()
        {
            Capacity = 300
        };
        BGAChanges = new List<BGAChange>()
        {
            Capacity = 10
        };
        BGSounds = new List<Note>();
        Lines = new Line[9];
        for (int i = 0; i < 9; ++i) Lines[i] = new Line();
    }

    public void AddBGAChange(int bar, int beat, int beatLength, int idx, bool isPic = false)
    {
        BGAChanges.Add(new BGAChange(bar, idx, beat, beatLength, isPic));
    }

    public void AddNote(int line, int bar, double beat, double beatLength, int keySound, int extra)
    {
        if(extra == -1) Lines[line].landMineList.Add(new Note(bar, keySound, beat, beatLength, extra));
        else Lines[line].noteList.Add(new Note(bar, keySound, beat, beatLength, extra));
    }

    public void AddBGSound(int bar, double beat, double beatLength, int keySound)
    {
        BGSounds.Add(new Note(bar, keySound, beat, beatLength, 0));
    }

    public void AddNewBeatC(int bar, double beatC)
    {
        BeatCTable.Add(bar, beatC);
    }

    public void AddBPM(int bar, double beat, double beatLength, double bpm)
    {
        Bpms.Add(new BPM(bar, bpm, beat, beatLength));
    }

    public void AddStop(int bar, double beat, double beatLength, string key)
    {
        Stops.Add(new Stop(bar, key, beat, beatLength));
    }

    public double GetPreviousBarBeatSum(int bar)
    {
		double sum = 0;
        for (int i = 0; i < bar; ++i)
        {
            sum += 4.0 * GetBeatC(i);
        }
        return sum;
    }

    private double GetTiming(Note note)
    {
        BPM prev = Bpms[Bpms.Count - 1];
		double sum = 0;
        for (int i = Bpms.Count - 1; i >= 0; --i)
        {
            double nextBeat = Bpms[i - 1].Beat;

            prev = Bpms[i - 1];
        }
        return sum;
    }

	public double GetBeatC(int bar) => BeatCTable.ContainsKey(bar) ? BeatCTable[bar] : 1.0;

    public void CalCulateBeats()
    {
        Debug.Log("Calc");

        foreach (BPM b in Bpms)
        {
            b.CalculateBeat(GetPreviousBarBeatSum(b.Bar), GetBeatC(b.Bar));
        }
        Bpms.Sort();
        if (Bpms.Count == 0 || (Bpms.Count > 0 && Bpms[Bpms.Count - 1].Beat != 0))
            AddBPM(0, 0, 1, BMSParser.instance.header.Bpm);

        Bpms[Bpms.Count - 1].Timing = 0;
        for (int i = Bpms.Count - 2; i > -1; --i)
        {
            Bpms[i].Timing = Bpms[i + 1].Timing + (Bpms[i].Beat - Bpms[i + 1].Beat) / (Bpms[i + 1].Bpm / 60);
        }

        foreach (Stop s in Stops)
        {
            s.CalculateBeat(GetPreviousBarBeatSum(s.Bar), GetBeatC(s.Bar));
            s.Timing = GetTimingInSecond(s);
        }
        Stops.Sort();

        foreach(BGAChange c in BGAChanges)
        {
            c.CalculateBeat(GetPreviousBarBeatSum(c.Bar), GetBeatC(c.Bar));
        }
        BGAChanges.Sort();

        foreach (Note n in BGSounds)
        {
            n.CalculateBeat(GetPreviousBarBeatSum(n.Bar), GetBeatC(n.Bar));
            n.Timing = GetTimingInSecond(n);
        }
        BGSounds.Sort();

        foreach (Line l in Lines)
        {
			int idx = Stops.Count - 1;

            foreach (Note n in l.landMineList)
            {
                n.CalculateBeat(GetPreviousBarBeatSum(n.Bar), GetBeatC(n.Bar));
				n.Timing = GetTimingInSecond(n);
            }
            foreach (Note n in l.noteList)
            {
                n.CalculateBeat(GetPreviousBarBeatSum(n.Bar), GetBeatC(n.Bar));
				n.Timing = GetTimingInSecond(n);
            }
            l.noteList.Sort();
            l.landMineList.Sort();
        }

        foreach (Line l in Lines)
        {
            for (int i = l.noteList.Count - 1; i > -1; --i)
            {
                Note n = l.noteList[i];


                Debug.Log($"{n.Timing} / BPM : {GetBPM(n.Beat)}");
            }
        }
    }

    private double GetBPM(double beat)
    {
        int idx = Bpms.Count - 1;
        while (idx > 0 && beat >= Bpms[--idx].Beat) ;
        return Bpms[idx + 1].Bpm;
    }

    private double GetTimingInSecond(BMSObject obj)
    {
        double timing = 0;
        int i;
        for (i = Bpms.Count - 1; i > 0 && obj.Beat > Bpms[i - 1].Beat; --i)
        {
            timing += (Bpms[i - 1].Beat - Bpms[i].Beat) / Bpms[i].Bpm * 60;
        }
        timing += (obj.Beat - Bpms[i].Beat) / Bpms[i].Bpm * 60;
        return timing;
    }

	public BMSObject Peek(List<BMSObject> list) => list[list.Count - 1];
}
