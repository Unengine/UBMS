using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BMSGameManager : MonoBehaviour
{
    public bool isAuto = false;
    public static float speed = 3.5f;

    [SerializeField]
    private UnityEngine.UI.Text comboText;
    [SerializeField]
    private Animator[] Explodes;
    [SerializeField]
    private double currentBeat = 0;
    [SerializeField]
    private double currentTime = 0;
    [SerializeField]
    private double scroll = 0;
    [SerializeField]
    private double stopTime = 0;
    private KeyCode[] Keys;
    [SerializeField]
    private Transform noteParent;
    [SerializeField]
    private VideoPlayer Video;
    [SerializeField]
    private bool isPaused = true;

    private JudgeManager judge;
    private Animator comboAnim;
    private BMSDrawer drawer;
    private BMSPattern pat;
    private SoundManager sm;
    private double currentBPM;
    private int combo = 0;

    // Use this for initialization
    private IEnumerator Start()
    {
        Application.runInBackground = true;
        pat = BMSParser.instance.pat;
        sm = GetComponent<SoundManager>();
        drawer = GetComponent<BMSDrawer>();
        drawer.DrawNotes();
        Debug.Log("GM Awake");
        Keys = new KeyCode[9];
        Keys[0] = KeyCode.S;
        Keys[1] = KeyCode.D;
        Keys[2] = KeyCode.F;
        Keys[3] = KeyCode.Space;
        Keys[4] = KeyCode.J;
        Keys[5] = KeyCode.LeftShift;
        Keys[6] = KeyCode.Escape;
        Keys[7] = KeyCode.K;
        Keys[8] = KeyCode.L;
        //StartCoroutine(MoveScroll());
        currentBPM = pat.Bpms[pat.Bpms.Count - 1].Bpm;
        pat.Bpms.RemoveAt(pat.Bpms.Count - 1);
        comboAnim = comboText.GetComponent<Animator>();
        judge = JudgeManager.instance;
        //Application.targetFrameRate = 60;

        Video.Prepare();
        yield return new WaitUntil(() => Video.isPrepared);
        Debug.Log("prepare done");
        Video.gameObject.GetComponent<UnityEngine.UI.RawImage>().texture = Video.texture;
        isPaused = false;

    }

    private void Update()
    {
        if (isPaused) return;

        for (int i = 0; i < Keys.Length; ++i)
        {
            if (pat.Lines[i].noteList.Count <= 0) continue;
            Note n = pat.Lines[i].noteList[pat.Lines[i].noteList.Count - 1];
            if (judge.Judge(n, currentTime) == JudgeType.IGNORE) continue;

            if (Input.GetKeyDown(Keys[i]) && pat.Lines[i].noteList[pat.Lines[i].noteList.Count - 1].Extra != 1)
            {
                HandleNote(pat.Lines[i], i);
            }
            else if (Input.GetKeyUp(Keys[i]) && pat.Lines[i].noteList[pat.Lines[i].noteList.Count - 1].Extra == 1)
            {
                HandleNote(pat.Lines[i], i);
            }

        }
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
		while (pat.BGAChanges.Count > 0 && pat.BGAChanges[pat.BGAChanges.Count - 1].Beat - 0.7f * currentBPM / 177 <= currentBeat)
        {
            if (!pat.BGAChanges[pat.BGAChanges.Count - 1].isPic)
            {
                Debug.Log("play");
                Video.Play();
            }

            pat.BGAChanges.RemoveAt(pat.BGAChanges.Count - 1);
        }

        double dt = Time.fixedDeltaTime;
		PlayNotes();
		if (stopTime > 0.0)
        {
            if (stopTime >= Time.fixedDeltaTime)
            {
                stopTime -= Time.fixedDeltaTime;
				return;
            }
            dt -= stopTime;
            stopTime = 0.0;
        }
		
		double avg = currentBPM;
		
		if (pat.Bpms.Count > 0)
		{
			avg = 0.0;
			BPM next = pat.Bpms[pat.Bpms.Count - 1];
			double prevTime = currentTime;
			while(pat.Bpms.Count > 0 && next.Timing <= currentTime + dt - stopTime)
			{
				if (pat.Stops.Count > 0)
				{
					Stop nextStop = pat.Stops[pat.Stops.Count - 1];
					while (pat.Stops.Count > 0 && nextStop.Timing < next.Timing)
					{
						double duration = pat.StopDurations[nextStop.Key] / currentBPM * 60 * 4;
						stopTime += duration;
						pat.Stops.RemoveAt(pat.Stops.Count - 1);
						if (pat.Stops.Count > 0)
							nextStop = pat.Stops[pat.Stops.Count - 1];
					}
				}
				double diff = next.Timing - prevTime;
				avg += currentBPM * (diff / dt);
				prevTime = next.Timing;
		        currentBPM = next.Bpm;
		        pat.Bpms.RemoveAt(pat.Bpms.Count - 1);
		        if (pat.Bpms.Count > 0) next = pat.Bpms[pat.Bpms.Count - 1];
		    }
		    if(prevTime <= currentTime + dt)
		    {
		        avg += currentBPM * ((currentTime + dt - prevTime) / dt);
		    }
		}

		if (pat.Stops.Count > 0)
		{
			Stop nextStop = pat.Stops[pat.Stops.Count - 1];
			while (pat.Stops.Count > 0 && pat.Stops[pat.Stops.Count - 1].Timing <= currentTime + dt)
			{
				double duration = pat.StopDurations[nextStop.Key] / currentBPM * 60 * 4;
				stopTime += duration;
				pat.Stops.RemoveAt(pat.Stops.Count - 1);
				if (pat.Stops.Count > 0)
					nextStop = pat.Stops[pat.Stops.Count - 1];
			}
		}

		currentBeat += avg / 60 * dt;
        currentTime += dt;
		scroll += avg / 60 * speed * dt;
        noteParent.transform.position = new Vector3(0.0f, (float)-scroll, 0.0f);	//손실을 적게 일어나게 하기 위해 누적된 double을 float로 변환.
	}

    private void HandleNote(Line l, int idx, float volume = 1.0f)
    {
        if (l.noteList.Count <= 0) return;
        Note n = l.noteList[l.noteList.Count - 1];
		n.model.SetActive(false);
		l.noteList.RemoveAt(l.noteList.Count - 1);
		if (n.Extra == -1)
		{
			//comboText.text = "LANDMINE!";
			return;
		}
		sm.PlayKeySound(n.KeySound, volume);
		JudgeType result = judge.Judge(n, currentTime);
        if (result > JudgeType.BAD)
        {
            comboText.text = result + " " + ++combo;
        }
        else
        {
            combo = 0;
            comboText.text = result.ToString();
        }

        if (combo > 0)
        {
            comboAnim.Rebind();
            comboAnim.Play("ComboUp");
            Explodes[idx].Rebind();
            Explodes[idx].Play("KeyExplode");
        }
    }

    private void PlayNotes()
    {
        while (pat.BGSounds.Count > 0 && pat.BGSounds[pat.BGSounds.Count - 1].Timing <= currentTime)
        {
            
            int keySound = pat.BGSounds[pat.BGSounds.Count - 1].KeySound;
            sm.PlayKeySound(keySound, 1.0f);
            pat.BGSounds.RemoveAt(pat.BGSounds.Count - 1);
        }

        if (isAuto)
        {
            for (int i = 0; i < pat.Lines.Length; ++i)
            {
                Line l = pat.Lines[i];
                while (l.noteList.Count > 0 && l.noteList[l.noteList.Count - 1].Timing <= currentTime)
                {
                    HandleNote(l, i);
                }
            }
        }

        while (pat.Lines[5].noteList.Count > 0 && pat.Lines[5].noteList[pat.Lines[5].noteList.Count - 1].Timing <= currentTime)
            HandleNote(pat.Lines[5], 5);

        if (!isAuto)
            for (int i = 0; i < pat.Lines.Length; ++i)
            {
                Line l = pat.Lines[i];
                while (l.noteList.Count > 0 && judge.Judge(l.noteList[l.noteList.Count - 1], currentTime) == JudgeType.POOR)
                {
                    combo = 0;
                    comboText.text = "POOR";
                    HandleNote(l, i, 0.3f);
                    //int keySound = l.noteQueue.Dequeue().KeySound;
                    //sm.PlayKeySound(keySound);
                }
            }
    }

    public void ToggleAuto()
    {
        isAuto = !isAuto;
    }
}
