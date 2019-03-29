using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class BMSGameManager : MonoBehaviour
{
	
	public bool isAuto = false;
	public static double scroll = 0;
	public static float speed = 7f;


	[SerializeField]
	private double AccuracySum = 0.0;
	[SerializeField]
	private int HitCount = 0;
	[SerializeField]
	private int score;
	[SerializeField]
	private UnityEngine.UI.Text FSText;
	[SerializeField]
	private UnityEngine.UI.Text BPMText;
	[SerializeField]
	private UnityEngine.UI.Text scoreText;
	[SerializeField]
	private UnityEngine.UI.Text comboText;
	[SerializeField]
	private Animator[] KeyPresses;
	[SerializeField]
	private Animator[] Explodes;
	[SerializeField]
	private double currentBeat = 0;
	[SerializeField]
	private double currentScrollTime = 0;
	[SerializeField]
	private double currentTime = 0;
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
		UpdateBPMText(currentBPM);
		comboAnim = comboText.GetComponent<Animator>();
		judge = JudgeManager.instance;

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
				if (Input.GetKeyDown(Keys[i]) && n.Extra != 1)
				{
					KeyPresses[i].Rebind();
					KeyPresses[i].Play("Press");
					if (judge.Judge(n, currentTime) != JudgeType.IGNORE)
						HandleNote(pat.Lines[i], i);
					sm.PlayKeySound(n.KeySound);
				}
				else if (Input.GetKeyUp(Keys[i]) && n.Extra == 1)
				{
					if (judge.Judge(n, currentTime) != JudgeType.IGNORE)
						HandleNote(pat.Lines[i], i);
					sm.PlayKeySound(n.KeySound);
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

		double prevStop = 0;
		double dt = Time.fixedDeltaTime;
		PlayNotes();
		currentTime += Time.fixedDeltaTime;
		if (stopTime > 0.0)
		{
			if (stopTime >= Time.fixedDeltaTime)
			{
				stopTime -= Time.fixedDeltaTime;
				return;
			}
			dt -= stopTime;
			prevStop = stopTime;
		}

		double avg = currentBPM * dt;

		BMSObject next = null;
		bool flag = false;

		if (pat.Stops.Count > 0)
		{
			next = pat.Stops[pat.Stops.Count - 1];
			if (next.Timing < currentScrollTime + dt)
			{
				flag = true;
				avg = 0;
			}
		}
		if (pat.Bpms.Count > 0)
		{
			BPM bpm = pat.Bpms[pat.Bpms.Count - 1];
			if (next == null) next = bpm;
			else if (bpm.Beat <= next.Beat) next = bpm;

			if (next.Timing < currentScrollTime + dt)
			{
				flag = true;
				avg = 0;
			}
		}


		double sub = 0;
		double prevTime = currentScrollTime;
		while (next != null && next.Timing + stopTime < currentScrollTime + Time.fixedDeltaTime)
		{
			if (next is BPM)
			{
				double diff = next.Timing - prevTime;
				avg += currentBPM * diff;
				currentBPM = (next as BPM).Bpm;
				UpdateBPMText(currentBPM);
				prevTime = next.Timing;
				pat.Bpms.RemoveAt(pat.Bpms.Count - 1);
			}
			if (next is Stop)
			{
				double diff = next.Timing - prevTime;
				avg += currentBPM * diff;
				prevTime = next.Timing;

				double duration = pat.StopDurations[(next as Stop).Key] / currentBPM * 240;
				stopTime += duration;
				pat.Stops.RemoveAt(pat.Stops.Count - 1);

				if(prevTime + stopTime >= currentScrollTime + dt)
				{
					double sdiff = currentScrollTime + dt - prevTime;
					sub += sdiff;
					stopTime -= sdiff;
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
				if (next == null) next = bpm;
				else if (bpm.Beat <= next.Beat) next = bpm;
			}
		}

		dt -= sub;
		if (dt < 0) Debug.LogWarning($"dt is negative! may be dangerous., {dt}");
		if (flag && prevTime <= currentScrollTime + dt)
		{
			avg += currentBPM * (currentScrollTime + dt - prevTime);
		}


		stopTime -= prevStop;
		avg /= 60;
		currentBeat += avg;
		currentScrollTime += dt;
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
		++HitCount;
		Note n = l.noteList[l.noteList.Count - 1];
		n.Model.SetActive(false);
		l.noteList.RemoveAt(l.noteList.Count - 1);

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

		UpdateScore(result);
		if (result != JudgeType.POOR)
			UpdateFSText((float)(n.Timing - currentTime) * 1000);
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
					sm.PlayKeySound(l.noteList[l.noteList.Count - 1].KeySound);
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
					Note n = l.noteList[l.noteList.Count - 1];
					sm.PlayKeySound(n.KeySound, 0.3f);
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
		{
			sm.PlayKeySound(pat.Lines[5].noteList[pat.Lines[5].noteList.Count - 1].KeySound);
			HandleNote(pat.Lines[5], 5);
		}


	}

	public void UpdateBPMText(double bpm)
	{
		if (bpm >= 1000 || bpm < 0) bpm = 0;
		BPMText.text = "BPM\n" + ((int)bpm).ToString("D3");
	}

	public void UpdateFSText(float diff)
	{
		if (Mathf.Abs(diff) <= 21.0)
		{
			FSText.text = string.Empty;
			return;
		}

		FSText.text = ((diff > 0) ? "FAST +" : "SLOW -") + Mathf.CeilToInt(Mathf.Abs(diff)) + "ms";
	}

	public void UpdateScore(JudgeType judge)
	{
		if (judge == JudgeType.PGREAT)
		{
			AccuracySum += 1;
			score += 2;
		}
		else if (judge == JudgeType.GREAT)
		{
			AccuracySum += 0.8;
			score += 1;
		}
		else if (judge == JudgeType.GOOD)
		{
			AccuracySum += 0.5;
		}

		scoreText.text = "SCORE : " + score.ToString("D4") + "\nACCURACY : " + (AccuracySum / HitCount).ToString("P");
	}

	public void ToggleAuto()
	{
		isAuto = !isAuto;
	}
}