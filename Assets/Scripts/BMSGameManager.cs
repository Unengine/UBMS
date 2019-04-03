using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class BMSGameManager : MonoBehaviour
{
	public bool IsAuto = true;
	public double Scroll;
	public static float Speed = 3f;
	public static bool IsPaused;

	[SerializeField]
	private BMSDrawer Drawer;
	[SerializeField]
	private GameUIManager GameUI;
	[SerializeField]
	private Transform NoteParent;
	[SerializeField]
	private VideoPlayer Video;
	[SerializeField]	
	private Animator[] KeyPresses;
	[SerializeField]
	private Animator[] Explodes;
	[SerializeField]
	private double AccuracySum = 0;
	[SerializeField]
	private double CurrentBeat = 0;
	[SerializeField]
	private double CurrentScrollTime = 0;
	[SerializeField]
	private double CurrentTime = 0;
	[SerializeField]
	private double StopTime = 0;
	[SerializeField]
	private int HitCount = 0;

	private BMSResult Res;
	private JudgeManager Judge;
	private BMSPattern Pat;
	private SoundManager Sm;
	private KeyCode[] Keys;
	private double CurrentBPM;
	private float Hp = 1;
	private int Combo = 0;

	private IEnumerator PreLoad()
	{
		BMSParser.Instance.Parse();
		Pat = BMSParser.Instance.Pat;

		Sm.AddAudioClips();
		GameUI.LoadImages();
		Pat.GetBeatsAndTimings();
		Drawer.DrawNotes();
		CurrentBPM = Pat.Bpms.Peek.Bpm;
		Pat.Bpms.RemoveLast();
				GameUI.UpdateBPMText(CurrentBPM);

		if (Video.isActiveAndEnabled)
		{
			for (int i = Pat.BGAChanges.Count - 1; i > -1; --i)
			{
				if (!Pat.BGAChanges[i].IsPic)
				{
					Video.url = "file://" + BMSFileSystem.SelectedHeader.ParentPath + "/" + Pat.BGVideoTable[Pat.BGAChanges[i].Key];
					break;
				}
			}

			Video.Prepare();
			yield return new WaitUntil(() => Video.isPrepared);
			Debug.Log("Video Prepared");

			Video.gameObject.GetComponent<UnityEngine.UI.RawImage>().texture = Video.texture;
			//Debug.Log(Video.clip.height + "/" + Video.clip.width);
			Video.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(600, Video.clip.height / Video.clip.width * 600);
		}

		yield return new WaitUntil(() => Sm.IsPrepared);
		IsPaused = false;
	}

	// Use this for initialization
	private void Awake()
	{
		Application.runInBackground = true;
		IsPaused = true;
		Pat = BMSParser.Instance.Pat;
		Sm = GetComponent<SoundManager>();
		Res = new BMSResult();
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
		Judge = JudgeManager.instance;

		StartCoroutine(PreLoad());
	}

	private void Update()
	{
		if (IsAuto) return;

		for (int i = 0; i < Keys.Length; ++i)
		{
			if(Input.GetKeyDown(Keys[i]))
			{
				KeyPresses[i].Rebind();
				KeyPresses[i].Play("Press");
			}

			if (Input.GetKey(Keys[i]))
			{
				KeyPresses[i].speed = 0;
			}
			else KeyPresses[i].speed = 1;

			if (IsPaused) continue;
			Note n = (Pat.Lines[i].NoteList.Count > 0) ? Pat.Lines[i].NoteList.Peek : null;
			if (Input.GetKeyDown(Keys[i]))
			{
				if (n != null && n.Extra != 1)
				{
					Sm.PlayKeySound(n.KeySound);
					if (Judge.Judge(n, CurrentTime) != JudgeType.IGNORE)
						HandleNote(Pat.Lines[i], i);
				}
			}
			else if (Input.GetKeyUp(Keys[i]))
			{
				if (n != null && n.Extra == 1)
				{
					Sm.PlayKeySound(n.KeySound);
					HandleNote(Pat.Lines[i], i);
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (IsPaused) return;
		while (Pat.BGAChanges.Count > 0 && Pat.BGAChanges.Peek.Timing - ((!Pat.BGAChanges.Peek.IsPic) ? 0.4 : 0) <= CurrentTime)
		{
			if (!Pat.BGAChanges.Peek.IsPic)
			{
				Video.Play();
			}
			else
			{
				GameUI.ChangeBGA(Pat.BGAChanges.Peek.Key);
			}
			Pat.BGAChanges.RemoveLast();
		}

		double prevStop = 0;
		double dt = Time.fixedDeltaTime;
		PlayNotes();
		CurrentTime += Time.fixedDeltaTime;
		if (StopTime > 0.0)
		{
			if (StopTime >= Time.fixedDeltaTime)
			{
				StopTime -= Time.fixedDeltaTime;
				return;
			}
			dt -= StopTime;
			prevStop = StopTime;
		}

		double avg = CurrentBPM * dt;

		BMSObject next = null;
		bool flag = false;

		if (Pat.Stops.Count > 0)
		{
			next = Pat.Stops.Peek;
			if (next.Timing < CurrentScrollTime + dt)
			{
				flag = true;
				avg = 0;
			}
		}
		if (Pat.Bpms.Count > 0)
		{
			BPM bpm = Pat.Bpms.Peek;
			if (next == null) next = bpm;
			else if (bpm.Beat <= next.Beat) next = bpm;

			if (next.Timing < CurrentScrollTime + dt)
			{
				flag = true;
				avg = 0;
			}
		}


		double sub = 0;
		double prevTime = CurrentScrollTime;
		while (next != null && next.Timing + StopTime < CurrentScrollTime + Time.fixedDeltaTime)
		{
			if (next is BPM)
			{
				double diff = next.Timing - prevTime;
				avg += CurrentBPM * diff;
				CurrentBPM = (next as BPM).Bpm;
				GameUI.UpdateBPMText(CurrentBPM);
				prevTime = next.Timing;
				Pat.Bpms.RemoveLast();
			}
			if (next is Stop)
			{
				double diff = next.Timing - prevTime;
				avg += CurrentBPM * diff;
				prevTime = next.Timing;

				double duration = Pat.StopDurations[(next as Stop).Key] / CurrentBPM * 240;
				StopTime += duration;
				Pat.Stops.RemoveLast();

				if(prevTime + StopTime >= CurrentScrollTime + dt)
				{
					double sdiff = CurrentScrollTime + dt - prevTime;
					sub += sdiff;
					StopTime -= sdiff;
					break;
				}
			}

			next = null;

			if (Pat.Stops.Count > 0)
			{
				next = Pat.Stops.Peek;
			}
			if (Pat.Bpms.Count > 0)
			{
				BPM bpm = Pat.Bpms.Peek;
				if (next == null) next = bpm;
				else if (bpm.Beat <= next.Beat) next = bpm;
			}
		}

		dt -= sub;
		if (dt < 0) Debug.LogWarning($"dt is negative! may be dangerous., {dt}");
		if (flag && prevTime <= CurrentScrollTime + dt)
		{
			avg += CurrentBPM * (CurrentScrollTime + dt - prevTime);
		}


		StopTime -= prevStop;
		avg /= 60;
		CurrentBeat += avg;
		CurrentScrollTime += dt;
		Scroll += avg * Speed;
		NoteParent.transform.position = new Vector3(0.0f, (float)-Scroll, 0.0f);
		//손실을 적게 일어나게 하기 위해 누적된 double을 float로 변환.
	}

	private void Damage(Line l)
	{
		ListExtension<Note> ListExtension = l.LandMineList;
		int idx = ListExtension.Count - 1;

		Note n = ListExtension[idx];
		if (n.Extra == -1 && Judge.Judge(n, CurrentTime) == JudgeType.PGREAT)
		{
			Debug.Log("bomb");
			GameUI.ComboUpTxt("LANDMINE!");
			n.Extra = -2;
		}

	}

	private void HandleNote(Line l, int idx, float volume = 1.0f)
	{
		if (l.NoteList.Count <= 0) return;
		++HitCount;
		Note n = l.NoteList.Peek;
		n.Model.SetActive(false);
		l.NoteList.RemoveLast();


		JudgeType result = Judge.Judge(n, CurrentTime);
		if (n.Extra == 1 && result == JudgeType.IGNORE)
		{
			result = JudgeType.POOR;
			n.Model.SetActive(false);
			l.NoteList.RemoveLast();
		}
		if (result > JudgeType.BAD)
		{
			GameUI.ComboUpTxt(result, ++Combo);
		}
		else
		{
			Combo = 0;
			GameUI.ComboUpTxt(result.ToString());
		}

		if (Combo > 0)
		{
			Explodes[idx].Rebind();
			Explodes[idx].Play("KeyExplode");
		}

		UpdateScore(result);
		if (result != JudgeType.POOR)
			GameUI.UpdateFSText((float)(n.Timing - CurrentTime) * 1000);
	}

	private void PlayNotes()
	{

		while (Pat.BGSounds.Count > 0 && Pat.BGSounds.Peek.Timing <= CurrentTime)
		{

			int keySound = Pat.BGSounds.Peek.KeySound;
			Sm.PlayKeySound(keySound, 1.0f);
			Pat.BGSounds.RemoveLast();
		}

		if (IsAuto)
		{
			for (int i = 0; i < Pat.Lines.Length; ++i)
			{
				Line l = Pat.Lines[i];
				while (l.NoteList.Count > 0 && l.NoteList.Peek.Timing <= CurrentTime)
				{
					KeyPresses[i].Rebind();
					KeyPresses[i].Play("Press");
					Sm.PlayKeySound(l.NoteList.Peek.KeySound);
					HandleNote(l, i);
				}

				while (l.LandMineList.Count > 0 &&
				Judge.Judge(l.LandMineList.Peek, CurrentTime) == JudgeType.POOR)
				{
					Note n = l.LandMineList.Peek;
					n.Model.SetActive(false);
					l.LandMineList.RemoveLast();
				}
			}
		}
		else
		{
			for (int i = 0; i < Pat.Lines.Length; ++i)
			{
				Line l = Pat.Lines[i];
				if (Input.GetKey(Keys[i]) && l.LandMineList.Count > 0)
				{
					Damage(l);
				}
				while (l.NoteList.Count > 0 && Judge.Judge(l.NoteList.Peek, CurrentTime) == JudgeType.POOR)
				{
					Note n = l.NoteList.Peek;
					Sm.PlayKeySound(n.KeySound, 0.3f);
					HandleNote(l, i, 0.3f);
				}

				while (l.LandMineList.Count > 0 &&
				Judge.Judge(l.LandMineList.Peek, CurrentTime) == JudgeType.POOR)
				{
					Note n = l.LandMineList.Peek;
					n.Model.SetActive(false);
					l.LandMineList.RemoveLast();
				}
			}
		}

		while (Pat.Lines[5].NoteList.Count > 0 && Pat.Lines[5].NoteList.Peek.Timing <= CurrentTime)
		{
			KeyPresses[5].Rebind();
			KeyPresses[5].Play("Press");
			Sm.PlayKeySound(Pat.Lines[5].NoteList.Peek.KeySound);
			HandleNote(Pat.Lines[5], 5);
		}


	}

	public void UpdateScore(JudgeType judge)
	{
		if (judge == JudgeType.PGREAT)
		{
			AccuracySum += 1;
			Res.Score += 2;
			++Res.Pgr;
			Hp += (float)BMSFileSystem.SelectedHeader.Total / 100 / Pat.NoteCount;
			if (Hp > 1) Hp = 1;
		}
		else if (judge == JudgeType.GREAT)
		{
			AccuracySum += 0.8;
			Res.Score += 1;
			++Res.Gr;
			Hp += (float)BMSFileSystem.SelectedHeader.Total / 100 / Pat.NoteCount;
			if (Hp > 1) Hp = 1;
		}
		else if (judge == JudgeType.GOOD)
		{
			AccuracySum += 0.5;
			++Res.Good;
			Hp += (float)BMSFileSystem.SelectedHeader.Total / 200 / Pat.NoteCount;
			if (Hp > 1) Hp = 1;
		}
		else if (judge == JudgeType.BAD)
		{
			++Res.Bad;
			Hp -= 0.02f;
			if (Hp < 0) Hp = 0;
		}
		else
		{
			++Res.Poor;
			Hp -= 0.04f;
			if (Hp < 0) Hp = 0;
		}

		GameUI.UpdateScore(Res, Hp, Res.Accaurcy = AccuracySum / HitCount);
	}

	public void ToggleAuto() => IsAuto = !IsAuto;
}