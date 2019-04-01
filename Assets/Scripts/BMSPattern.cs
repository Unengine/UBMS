using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BMSPattern
{
	public int NoteCount { get; set; } = 0;
	public int BarCount { get; set; } = 0;
	public ListExtension<BGChange> BGAChanges { get; set; }
	public ListExtension<Note> BGSounds { get; set; }
	public ListExtension<BPM> Bpms { get; set; }
	public ListExtension<Stop> Stops { set; get; }
	public Dictionary<string, double> StopDurations { get; set; }
	public Dictionary<int, double> BeatCTable { get; set; }
	public Dictionary<string, string> BGVideoTable { get; set; }
	public Line[] Lines { get; set; }

	public BMSPattern()
	{
		BeatCTable = new Dictionary<int, double>();
		StopDurations = new Dictionary<string, double>();
		BGVideoTable = new Dictionary<string, string>();
		Stops = new ListExtension<Stop>()
		{
			Capacity = 5
		};
		Bpms = new ListExtension<BPM>()
		{
			Capacity = 5
		};
		BGAChanges = new ListExtension<BGChange>()
		{
			Capacity = 10
		};
		BGSounds = new ListExtension<Note>();
		Lines = new Line[9];
		for (int i = 0; i < 9; ++i) Lines[i] = new Line();
	}

	public void AddBGAChange(int bar, double beat, double beatLength, string key, bool isPic = false)
		=> BGAChanges.Add(new BGChange(bar, key, beat, beatLength, isPic));

	public void AddNote(int line, int bar, double beat, double beatLength, int keySound, int extra)
	{
		if (extra == -1) Lines[line].LandMineList.Add(new Note(bar, keySound, beat, beatLength, extra));
		else
		{
			++NoteCount;
			Lines[line].NoteList.Add(new Note(bar, keySound, beat, beatLength, extra));
		}
	}

	public void AddBGSound(int bar, double beat, double beatLength, int keySound)
		=> BGSounds.Add(new Note(bar, keySound, beat, beatLength, 0));

	public void AddNewBeatC(int bar, double beatC)
		=> BeatCTable.Add(bar, beatC);

	public void AddBPM(int bar, double beat, double beatLength, double bpm)
		=> Bpms.Add(new BPM(bar, bpm, beat, beatLength));

	public void AddBPM(BPM bpm)
		=> Bpms.Add(bpm);

	public void AddStop(int bar, double beat, double beatLength, string key)
		=> Stops.Add(new Stop(bar, key, beat, beatLength));

	public double GetBeatC(int bar) => BeatCTable.ContainsKey(bar) ? BeatCTable[bar] : 1.0;

	public void GetBeatsAndTimings()
	{
		foreach (BPM b in Bpms) b.CalculateBeat(GetPreviousBarBeatSum(b.Bar), GetBeatC(b.Bar));
		Bpms.Sort();
		if (Bpms.Count == 0 || (Bpms.Count > 0 && Bpms[Bpms.Count - 1].Beat != 0))
			AddBPM(0, 0, 1, BMSParser.Instance.Header.Bpm);

		Bpms[Bpms.Count - 1].Timing = 0;
		for (int i = Bpms.Count - 2; i > -1; --i)
		{
			Bpms[i].Timing = Bpms[i + 1].Timing + (Bpms[i].Beat - Bpms[i + 1].Beat) / (Bpms[i + 1].Bpm / 60);
		}
		//GET BPM

		foreach (Stop s in Stops)
		{
			s.CalculateBeat(GetPreviousBarBeatSum(s.Bar), GetBeatC(s.Bar));
			s.Timing = GetTimingInSecond(s);
		}
		Stops.Sort();
		//GET STOP

		foreach (BGChange c in BGAChanges)
		{
			c.CalculateBeat(GetPreviousBarBeatSum(c.Bar), GetBeatC(c.Bar));
			c.Timing = GetTimingInSecond(c);
		}
		BGAChanges.Sort();
		//GET BGCHANGE

		CalCulateTimingsInListExtension(BGSounds);
		//GET BGSOUND

		foreach (Line l in Lines)
		{
			CalCulateTimingsInListExtension(l.NoteList);
			CalCulateTimingsInListExtension(l.LandMineList);
		}
		//GET NOTES
	}

	public void CalCulateTimingsInListExtension(ListExtension<Note> list)
	{
		foreach(Note n in list)
		{
			n.CalculateBeat(GetPreviousBarBeatSum(n.Bar), GetBeatC(n.Bar));
			n.Timing = GetTimingInSecond(n);
			int idx = Stops.Count;
			double sum = 0;
			while (idx > 0 && n.Beat > Stops[--idx].Beat) sum += StopDurations[Stops[idx].Key] / GetBPM(Stops[idx].Beat) * 240;
			n.Timing += sum;
			//Add stoptime
		}
		list.Sort();
	}

	private double GetBPM(double beat)
	{
		if (Bpms.Count == 1) return Bpms[0].Bpm;
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
}