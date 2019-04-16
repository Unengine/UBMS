using UnityEngine;

public class Line
{
	public ListExtension<Note> NoteList;
	public ListExtension<Note> LandMineList;
	public Line()
	{
		NoteList = new ListExtension<Note>()
		{
			Capacity = 225
		};
		LandMineList = new ListExtension<Note>()
		{
			Capacity = 20
		};
	}
}

public abstract class BMSObject : System.IComparable<BMSObject>
{
	public int Bar { get; protected set; }
	public double Beat { get; protected set; }
	public double Timing { get; set; }

	public BMSObject(int bar, double beat, double beatLength)
	{
		Bar = bar;
		Beat = (beat / beatLength) * 4.0;
	}

	public BMSObject(int bar, double beat)
	{
		Bar = bar;
		Beat = beat;
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

public class BGChange : BMSObject
{
	public string Key { get; private set; }
	public bool IsPic { get; set; }

	public BGChange(int bar, string key, double beat, double beatLength, bool isPic) : base(bar, beat, beatLength)
	{
		Key = key;
		IsPic = isPic;
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

	public BPM(BPM bpm) : base(bpm.Bar, bpm.Beat)
	{
		Bpm = bpm.Bpm;
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

public class BMSResult
{
	public int NoteCount { get; set; }
	public int Pgr { get; set; }
	public int Gr { get; set; }
	public int Good { get; set; }
	public int Bad { get; set; }
	public int Poor { get; set; }
	public int Score { get; set; }
	public double Accuracy { get; set; }

}

public class Utility {

	public static double DAbs(double value) => (value > 0) ? value : -value;

	public static int DCeilToInt(double value) => (int)(value + 1);
}

public enum GaugeType
{
	Easy,
	Groove,
	Hard,
	EXHard
}

public class Gauge
{
	private readonly GaugeType Type;
	public float Hp { get; set; }
	public float GreatHealAmount { get; set; }
	public float GoodHealAmount { get; set; } = 0;
	public float BadDamage { get; }
	public float PoorDamage { get; }
	public Gauge(GaugeType type, float total, int noteCount)
	{
		Type = type;

		if (type == GaugeType.Groove)
		{
			Hp = 0.2f;
			GreatHealAmount = total / noteCount;
			GoodHealAmount = GreatHealAmount / 2;
			BadDamage = 0.04f;
			PoorDamage = 0.06f;
		}
		else if (type == GaugeType.Easy)
		{
			Hp = 0.2f;
			GreatHealAmount = total / noteCount * 1.2f;
			GoodHealAmount = GreatHealAmount / 2;
			BadDamage = 0.032f;
			PoorDamage = 0.08f;
		}
		else if(type == GaugeType.Hard)
		{
			Hp = 1;
			GreatHealAmount = 0.1f;
			BadDamage = 0.06f;
			PoorDamage = 0.1f;
		}
		else if(type == GaugeType.EXHard)
		{
			Hp = 1;
			GreatHealAmount = 0.1f;
			BadDamage = 0.1f;
			PoorDamage = 0.18f;
		}
		GreatHealAmount /= 100;
		GoodHealAmount /= 100;
	}
}