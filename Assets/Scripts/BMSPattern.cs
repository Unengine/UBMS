using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGAChange : BMSObject
{
    public int Idx { get; set; } = 0;
    public bool isPic;
    public BGAChange(int bar, int idx, float beat, float beatLength, bool isPic) : base(bar, beat, beatLength)
    {
        Idx = idx;
    }
}

public class Line
{
    public List<Note> noteList;
    public Line()
    {
        noteList = new List<Note>();
    }
}

public abstract class BMSObject : IComparable<BMSObject>
{
    public int Bar { get; protected set; }
    public float Beat { get; protected set; }
    public float Timing { get; set; }

    public BMSObject(int bar, float beat, float beatLength)
    {
        Bar = bar;
        Beat = (beat / beatLength) * 4.0f;
    }

    public void CalculateBeat(float prevBeats, float beatC)
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
    public int Extra { get; private set; }
    public GameObject model { get; set; }

    public Note(int bar, int keySound, float beat, float beatLength, int extra) : base(bar, beat, beatLength)
    {
        KeySound = keySound;
        Extra = extra;
    }
}

public class BPM : BMSObject
{
    public float Bpm { get; private set; }

    public BPM(int bar, float bpm, float beat, float beatLength) : base(bar, beat, beatLength)
    {
        Bpm = bpm;
    }
}

public class Stop : BMSObject
{
    public int Idx;
    public Stop(int bar, int idx, float beat, float beatLength) : base(bar, beat, beatLength)
    {
        Idx = idx;
    }
}

public class BMSPattern {

    public int BarCount { get; set; } = 0;
    public List<BGAChange> BGAChanges { get; set; }
    public List<Note> BGSounds { get; set; }
    public List<string> KeySounds { get; set; }
    public List<BPM> Bpms { get; set; }
    public List<Stop> Stops { set; get; }
    public List<float> StopDurations { get; set; }
    public Dictionary<int, float> BeatCTable { get; set; }
    public Line[] Lines { get; set; }

    public BMSPattern()
    {
        BeatCTable = new Dictionary<int, float>();
        StopDurations = new List<float>()
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

    public void AddNote(int line, int bar, float beat, float beatLength, int keySound, int extra)
    {
        Lines[line].noteList.Add(new Note(bar, keySound, beat, beatLength, extra));
    }

    public void AddBGSound(int bar, float beat, float beatLength, int keySound)
    {
        BGSounds.Add(new Note(bar, keySound, beat, beatLength, 0));
    }

    public void AddNewBeatC(int bar, float beatC)
    {
        BeatCTable.Add(bar, beatC);
    }

    public void AddBPM(int bar, float beat, float beatLength, float bpm)
    {
        Bpms.Add(new BPM(bar, bpm, beat, beatLength));
    }

    public void AddStop(int bar, float beat, float beatLength, int idx)
    {
        Stops.Add(new Stop(bar, idx, beat, beatLength));
    }

    public float GetPreviousBarBeatSum(int bar)
    {
        float sum = 0;
        for (int i = 0; i < bar; ++i)
        {
            sum += 4.0f * BeatCTable[i];
        }
        return sum;
    }

    private float GetTiming(Note note)
    {
        BPM prev = Bpms[Bpms.Count - 1];
        float sum = 0;
        for (int i = Bpms.Count - 1; i >= 0; --i)
        {
            float nextBeat = Bpms[i - 1].Beat;

            prev = Bpms[i - 1];
        }
        return sum;
    }

    public void CalCulateBeats()
    {
        Debug.Log("Calc");

        for (int i = 0; i <= BarCount; ++i)
            if (!BeatCTable.ContainsKey(i))
                BeatCTable.Add(i, 1.0f);

        foreach (BPM b in Bpms)
        {
            b.CalculateBeat(GetPreviousBarBeatSum(b.Bar), BeatCTable[b.Bar]);
        }
        Bpms.Sort();
        if (Bpms.Count == 0 || (Bpms.Count > 0 && Bpms[Bpms.Count - 1].Beat != 0))
            AddBPM(0, 0, 1, BMSParser.instance.header.Bpm);

        Bpms[Bpms.Count - 1].Timing = 0;
        for (int i = Bpms.Count - 2; i > -1; --i)
        {
            Bpms[i].Timing += Bpms[i + 1].Timing + (Bpms[i].Beat - Bpms[i + 1].Beat) / (Bpms[i + 1].Bpm * 0.0166666666f);
        }

        foreach (Stop s in Stops)
        {
            s.CalculateBeat(GetPreviousBarBeatSum(s.Bar), BeatCTable[s.Bar]);
            s.Timing = GetTimingInSecond(s);
        }
        Stops.Sort();

        foreach(BGAChange c in BGAChanges)
        {
            c.CalculateBeat(GetPreviousBarBeatSum(c.Bar), BeatCTable[c.Bar]);
        }
        BGAChanges.Sort();


        foreach (Note n in BGSounds)
        {
            n.CalculateBeat(GetPreviousBarBeatSum(n.Bar), BeatCTable[n.Bar]);
            n.Timing = GetTimingInSecond(n);
        }
        BGSounds.Sort();

        foreach (Line l in Lines)
        {
            foreach (Note n in l.noteList)
            {
                n.CalculateBeat(GetPreviousBarBeatSum(n.Bar), BeatCTable[n.Bar]);
                n.Timing = GetTimingInSecond(n);
                //n.Timing = GetTiming(n.Beat);
            }
            l.noteList.Sort();
        }
    }

    private float GetBPM(float beat)
    {
        int i;
        for (i = Bpms.Count - 1; i > 0 && beat > Bpms[i - 1].Beat; --i) ;
        return Bpms[i].Bpm;
    }

    private float GetTimingInSecond(BMSObject obj)
    {
        float timing = 0.0f;
        int i;
        for (i = Bpms.Count - 1; i > 0 && obj.Beat > Bpms[i - 1].Beat; --i)
        {
            timing += (Bpms[i - 1].Beat - Bpms[i].Beat) / (Bpms[i].Bpm * 0.016666666f);
        }
        timing += (obj.Beat - Bpms[i].Beat) / (Bpms[i].Bpm * 0.016666666f);
        return timing;
    }
}
