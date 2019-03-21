using System;
using System.Collections;
using System.Collections.Generic;

public class BMSPattern_Legacy {

    public struct BGNote : IComparable<BGNote>
    {
        public int KeySound;
        public int Bar;
        public int Beat;
        public int BeatLength;
        public float timing;
        public BGNote(int bar, int beat, int beatLength, int keySound, float bpm, float barLength, float prevTime = 0)
        {
            Bar = bar;
            Beat = beat;
            BeatLength = beatLength;
            KeySound = keySound;
            timing = prevTime + (barLength * bpm / 60) / (bpm / 60 * 0.25f) / beatLength * beat;
        }

        public int CompareTo(BGNote other)
        {
            if (timing < other.timing) return 1;
            if (timing == other.timing) return 0;
            return -1;
        }
    }

    public struct Note
    {
        public int KeySound;
        public int Bar;
        public int Beat;
        public int BeatLength;
        public int extra;
        public float timing;
        public Note(int bar, int beat, int beatLength, int keySound, float bpm, float barLength, float prevTime = 0)
        {
            Bar = bar;
            Beat = beat;
            BeatLength = beatLength;
            KeySound = keySound;
            extra = 0;
            timing = prevTime + (barLength * bpm / 60) / (bpm / 60 * 0.25f) / beatLength * beat;
        }
    }

    public class Line
    {
        public int prevBar = 0;
        public float prevTime = 0;
        public Queue<Note> noteQueue;
        public Line()
        {
            noteQueue = new Queue<Note>();
        }
    }

    public List<BGNote> BGSounds { get; set; }
    public List<string> KeySounds { get; set; }
    public List<float> Bpms { get; set; }
    public List<float> Stops { set; get; }
    public List<float> BarLength { get; set; }
    public Line[] Lines { get; set; }
    private int bpmIdx = 0;
    public int NoteCount { get; set; }
    public float delay = 0;
    
    public BMSPattern_Legacy()
    {
        BarLength = new List<float>()
        {
            Capacity = 5
        };
        Stops = new List<float>()
        {
            Capacity = 5
        };
        Bpms = new List<float>()
        {
            Capacity = 10
        };
        KeySounds = new List<string>()
        {
            Capacity = 300
        };
        BGSounds = new List<BGNote>();
        Lines = new Line[9];
        for (int i = 0; i < 9; ++i) Lines[i] = new Line();
    }

    public void AddNote(int line, int bar, int beat, int beatLength, int keySound, float barLength)
    {
        //마지막에서 3번째, 1번째 인자 바뀌어야함
        Lines[line].noteQueue.Enqueue(new Note(bar, beat, beatLength, keySound, 180, barLength, bar * (barLength * 180 * 0.016666f) / (180 * 0.016666f * 0.25f)));
        ++NoteCount;
    }

    public void AddBGSound(int bar, int beat, int beatLength, int keySound, float barLength)
    {
        BGSounds.Add(new BGNote(bar, beat, beatLength, keySound, 180, barLength, bar * (barLength * 180 * 0.016666f) / (180 * 0.016666f * 0.25f)));
    }

    public void SortBGSound()
    {
        BGSounds.Sort();
    }
}
