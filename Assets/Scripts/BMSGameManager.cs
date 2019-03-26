 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BMSGameManager : MonoBehaviour
{
	public bool isAuto = false;
	public static float speed = 7f;

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
		Application.targetFrameRate = 120;

		Video.Prepare();
		yield return new WaitUntil(() => Video.isPrepared);
		Debug.Log("prepare done");
		Video.gameObject.GetComponent<UnityEngine.UI.RawImage>().texture = Video.texture;
		isPaused = false;

	}

	private void Update()
	{
		if (isPaused || isAuto) return;

		for (int i = 0; i < Keys.Length; ++i)
		{
			if (pat.Lines[i].noteList.Count > 0)
			{
                Note n = pat.Lines[i].noteList[pat.Lines[i].noteList.Count - 1];
                if (judge.Judge(n, currentTime) != JudgeType.IGNORE)
                {
                    if (Input.GetKeyDown(Keys[i]) && n.Extra != 1)
                    {
                        HandleNote(pat.Lines[i], i);
                    }
                    else if (Input.GetKeyUp(Keys[i]) && n.Extra == 1)
                    {
                        HandleNote(pat.Lines[i], i);
                    }
                }
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



		double avg = currentBPM * dt;

		BMSObject next = null;
		BPM nextBPM = null;
		bool flag = false;


		if(pat.Stops.Count > 0)
		{
			next = pat.Stops[pat.Stops.Count - 1];
		}
		if(pat.Bpms.Count > 0)
		{
			BPM bpm = pat.Bpms[pat.Bpms.Count - 1];
			if(next == null) next = nextBPM = bpm;
			else if(bpm.Beat <= next.Beat) next = nextBPM = bpm;

			if(next.Timing < currentTime + dt)
			{
				flag = true;
				avg = 0;
			}
		}


		double sub = 0;
		double prevTime = currentTime;
        while (next != null && next.Timing + stopTime < currentTime + dt)
        {
			if(next is BPM)
			{
				//Debug.Log($"change bpm to {nextBPM.Bpm} in {next.Bar}");
				double diff = nextBPM.Timing - prevTime;
				avg += currentBPM * diff;
				currentBPM = nextBPM.Bpm;
				prevTime = nextBPM.Timing;
                pat.Bpms.RemoveAt(pat.Bpms.Count - 1);
			}
			if(next is Stop)
			{
				//Debug.Log($"stop in {next.Bar}");
				double duration = pat.StopDurations[(next as Stop).Key] / currentBPM * 240;
				stopTime += duration;
				pat.Stops.RemoveAt(pat.Stops.Count - 1);
				if(next.Timing + stopTime >= currentTime + dt)
				{
					double diff = currentTime + dt - next.Timing;
					sub += diff;
					stopTime -= diff;
					break;
				}
			}

			next = null;

            if (pat.Stops.Count > 0)
            {
                next = pat.Stops[pat.Stops.Count - 1];
            }
            if (pat.Bpms.Count > 0)
            {
                BPM bpm = pat.Bpms[pat.Bpms.Count - 1];
                if (next == null) next = nextBPM = bpm;
                else if (bpm.Beat <= next.Beat) next = nextBPM = bpm;
            }
        }

		dt -= sub;
		if(dt < 0) Debug.Log($"fuck, {dt}");
        if (flag && prevTime <= currentTime + dt)
        {
            avg += currentBPM * (currentTime + dt - prevTime);
        }

		avg /= 60;
		currentBeat += avg;
		currentTime += dt;
		scroll += avg * speed;
		noteParent.transform.position = new Vector3(0.0f, (float)-scroll, 0.0f);
		//손실을 적게 일어나게 하기 위해 누적된 double을 float로 변환.
	}

	private void Damage(Line l)
	{
		List<Note> list = l.landMineList;
		int idx = list.Count - 1;

        Note n = list[idx];
        if (n.Extra == -1 && judge.Judge(n, currentTime) == JudgeType.PGREAT)
        {
            Debug.Log("bomb");
            comboText.text = "LANDMINE!";
            comboAnim.Rebind();
            comboAnim.Play("ComboUp");
            n.Extra = -2;
        }
		
	}

	private void HandleNote(Line l, int idx, float volume = 1.0f)
	{
		if (l.noteList.Count <= 0) return;
        Note n = l.noteList[l.noteList.Count - 1];
        n.Model.SetActive(false);
        l.noteList.RemoveAt(l.noteList.Count - 1);

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
				
				while (l.landMineList.Count > 0 &&
				judge.Judge(l.landMineList[l.landMineList.Count - 1], currentTime) == JudgeType.POOR)
				{
                    Note n = l.landMineList[l.landMineList.Count - 1];
                    n.Model.SetActive(false);
                    l.landMineList.RemoveAt(l.landMineList.Count - 1);
				}
			}
		}
		else
		{
			for (int i = 0; i < pat.Lines.Length; ++i)
			{
				Line l = pat.Lines[i];
            	if (Input.GetKey(Keys[i]) && l.landMineList.Count > 0)
                {
                    Damage(l);
                }
				while (l.noteList.Count > 0 && judge.Judge(l.noteList[l.noteList.Count - 1], currentTime) == JudgeType.POOR)
				{
					HandleNote(l, i, 0.3f);
				}
				
                while (l.landMineList.Count > 0 &&
				judge.Judge(l.landMineList[l.landMineList.Count - 1], currentTime) == JudgeType.POOR)
				{
                    Note n = l.landMineList[l.landMineList.Count - 1];
                    n.Model.SetActive(false);
                    l.landMineList.RemoveAt(l.landMineList.Count - 1);
				}
			}
		}

		while (pat.Lines[5].noteList.Count > 0 && pat.Lines[5].noteList[pat.Lines[5].noteList.Count - 1].Timing <= currentTime)
			HandleNote(pat.Lines[5], 5);


	}

	public void ToggleAuto()
	{
		isAuto = !isAuto;
	}
}
