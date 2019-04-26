using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class BMSGameManager : MonoBehaviour
{
	public static BMSResult Res;
	public static int JudgeAdjValue = 0;
	public static float Speed = 3f;
	public static bool IsBgaOn = true;
	public static bool IsPaused;
	public static bool WillSaveData;
	public static bool IsAutoScr = true;
	public bool IsAuto = false;
	public double Scroll;

	[SerializeField]
	private BMSDrawer Drawer;
	[SerializeField]
	private GameUIManager UI;
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

	public static BMSHeader Header;
	private Gauge Gauge;
	private JudgeManager Judge;
	private BMSPattern Pat;
	private SoundManager Sm;
	private WaitForSeconds Wait2Sec;
	private KeyCode[] Keys;
	private double CurrentBPM;
	private int Combo = 0;
	private bool IsBgaVideoSupported = false;

	private IEnumerator PreLoad()
	{
		BMSParser.Instance.Parse();
		Pat = BMSParser.Instance.Pat;
		Pat.GetBeatsAndTimings();
		Gauge = new Gauge(SelUIManager.Gauge, Header.Total, Pat.NoteCount);
		UI.SetHPBarSprite(Gauge.Type);
		UI.UpdateScore(Res, Gauge.Hp, 0.0f);
		UI.UpdateComboText("Loading...");
		UI.LoadBackBmp();
		UI.Bga.rectTransform.sizeDelta = new Vector2(298, 349);
		Sm.AddAudioClips();
		Res.NoteCount = Pat.NoteCount;
		Drawer.DrawNotes();
		CurrentBPM = Pat.Bpms.Peek.Bpm;
		Pat.Bpms.RemoveLast();
		UI.UpdateBPMText(CurrentBPM);

		if (IsBgaOn)
		{
			UI.LoadImages();

			for (int i = Pat.BGAChanges.Count - 1; i > -1; --i)
			{
				if (!Pat.BGAChanges[i].IsPic)
				{
					Video.url = "file://" + Header.ParentPath + "/" + Pat.BGVideoTable[Pat.BGAChanges[i].Key];
					break;
				}
			}

			if (!string.IsNullOrEmpty(Video.url))
			{
				bool errorFlag = false;
				Video.errorReceived += (a, b) => errorFlag = true;
				Video.Prepare();
				yield return new WaitUntil(() => (Video.isPrepared || errorFlag));
				Debug.Log("Video Prepared");
				IsBgaVideoSupported = !errorFlag;
			}
			yield return new WaitUntil(() => UI.IsPrepared);
		}
		yield return new WaitUntil(() => Sm.IsPrepared);
		Debug.Log("Game starts in 2 sec");
		UI.ComboUpTxt("Game Start!");
		yield return Wait2Sec;
		UI.UpdateComboText(string.Empty);
		if (IsBgaOn && IsBgaVideoSupported)
		{
			UI.Bga.texture = Video.texture;
			UI.Bga.color = Color.white;
			UI.Bga.rectTransform.sizeDelta = new Vector2(600, 600);
		}
		StartCoroutine(CheckIfSongEnded());
		CurrentTime += (JudgeAdjValue / 1000.0f);
		IsPaused = false;
	}

	// Use this for initialization
	private void Awake()
	{
		WillSaveData = !IsAutoScr;
		IsAuto = false;
		IsPaused = true;
		Pat = BMSParser.Instance.Pat;
		Sm = GetComponent<SoundManager>();
		Res = new BMSResult();
		Keys = new KeyCode[10];
		Keys[0] = (KeyCode)KeySettingManager.Config.Keys[2];
		Keys[1] = (KeyCode)KeySettingManager.Config.Keys[3];
		Keys[2] = (KeyCode)KeySettingManager.Config.Keys[4];
		Keys[3] = (KeyCode)KeySettingManager.Config.Keys[5];
		Keys[4] = (KeyCode)KeySettingManager.Config.Keys[6];
		Keys[5] = (KeyCode)KeySettingManager.Config.Keys[0];	//sc up
		Keys[6] = KeyCode.None;
		Keys[7] = (KeyCode)KeySettingManager.Config.Keys[7];
		Keys[8] = (KeyCode)KeySettingManager.Config.Keys[8];
		Keys[9] = (KeyCode)KeySettingManager.Config.Keys[1];	//sc down
		Judge = JudgeManager.instance;
		Header = BMSFileSystem.SelectedHeader;
		BMSFileSystem.SelectedHeader = null;
		BMSFileSystem.SelectedPath = null;
		UI.Bga.color = new Color(1, 1, 1, 0);
		Wait2Sec = new WaitForSeconds(2.0f);
		StartCoroutine(PreLoad());
	}

	private void Update()
	{
		if (IsAuto) return;

		for (int i = 0; i < Keys.Length; ++i)
		{
			int lineIdx = (i == 9) ? 5 : i;
			if(Input.GetKeyDown(Keys[i]))
			{
				KeyPresses[lineIdx].Rebind();
				KeyPresses[lineIdx].Play("Press");
			}

			if (Input.GetKey(Keys[i]))
			{
				KeyPresses[lineIdx].speed = 0;
			}
			else KeyPresses[lineIdx].speed = 1;

			if (IsPaused || (IsAutoScr && lineIdx == 5)) continue;
			Note n = (Pat.Lines[lineIdx].NoteList.Count > 0) ? Pat.Lines[lineIdx].NoteList.Peek : null;
			if (Input.GetKeyDown(Keys[i]))
			{
				if (n != null && n.Extra != 1)
				{
					Sm.PlayKeySound(n.KeySound);
					if (Judge.Judge(n, CurrentTime) != JudgeType.IGNORE)
						HandleNote(Pat.Lines[lineIdx], lineIdx);
				}
			}
			else if (Input.GetKeyUp(Keys[i]))
			{
				if (n != null && n.Extra == 1)
				{
					Sm.PlayKeySound(n.KeySound);
					HandleNote(Pat.Lines[lineIdx], lineIdx);
				}
			}
		}
	}

	private void FixedUpdate()
	{
		if (IsPaused) return;
		while (Pat.BGAChanges.Count > 0 && Pat.BGAChanges.Peek.Timing - ((!Pat.BGAChanges.Peek.IsPic) ? 0.4 : 0) <= CurrentTime)
		{
			if (IsBgaVideoSupported && !Pat.BGAChanges.Peek.IsPic)
			{
				Video.Play();
			}
			else
			{
				UI.ChangeBGA(Pat.BGAChanges.Peek.Key);
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
				UI.UpdateBPMText(CurrentBPM);
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
			UI.ComboUpTxt("LANDMINE!");
			n.Extra = -2;
		}

	}

	private void HandleNote(Line l, int idx, float volume = 1.0f, bool IsScoreAdded = true)
	{
		if (l.NoteList.Count <= 0) return;
		++HitCount;
		Note n = l.NoteList.Peek;
		n.Model.SetActive(false);
		l.NoteList.RemoveLast();

		JudgeType result = Judge.Judge(n, CurrentTime);
		if(l.NoteList.Count > 0 && l.NoteList.Peek.Extra == 1 && result == JudgeType.POOR)
		{
			++HitCount;
			l.NoteList.Peek.Model.SetActive(false);
			l.NoteList.RemoveLast();
		}
		if (!IsScoreAdded) return;
		if (n.Extra == 1 && result == JudgeType.IGNORE)
		{
			result = JudgeType.POOR;
			//++HitCount;
			n.Model.SetActive(false);
			l.NoteList.RemoveLast();
		}
		if (result > JudgeType.BAD)
		{
			UI.ComboUpTxt(result, ++Combo);
		}
		else
		{
			Combo = 0;
			UI.ComboUpTxt(result.ToString());
		}

		if (Combo > 0)
		{
			Explodes[idx].Rebind();
			Explodes[idx].Play("KeyExplode");
		}

		if (IsScoreAdded)
			UpdateScore(result);
		if (result != JudgeType.POOR)
			UI.UpdateFSText((float)(n.Timing - CurrentTime) * 1000);
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

		if (IsAutoScr)
			while (Pat.Lines[5].NoteList.Count > 0 && Pat.Lines[5].NoteList.Peek.Timing <= CurrentTime)
			{
				KeyPresses[5].Rebind();
				KeyPresses[5].Play("Press");
				Sm.PlayKeySound(Pat.Lines[5].NoteList.Peek.KeySound);
				HandleNote(Pat.Lines[5], 5, 1, false);
			}


	}

	public void UpdateScore(JudgeType judge)
	{
		if (judge == JudgeType.PGREAT)
		{
			AccuracySum += 1;
			Res.Score += 2;
			++Res.Pgr;
			Gauge.Hp += Gauge.GreatHealAmount;
			if (Gauge.Hp > 1) Gauge.Hp = 1;
		}
		else if (judge == JudgeType.GREAT)
		{
			AccuracySum += 0.8;
			Res.Score += 1;
			++Res.Gr;
			Gauge.Hp += Gauge.GreatHealAmount;
			if (Gauge.Hp > 1) Gauge.Hp = 1;
		}
		else if (judge == JudgeType.GOOD)
		{
			AccuracySum += 0.5;
			++Res.Good;
			Gauge.Hp += Gauge.GoodHealAmount;
			if (Gauge.Hp > 1) Gauge.Hp = 1;
		}
		else if (judge == JudgeType.BAD)
		{
			++Res.Bad;
			Gauge.Hp -= Gauge.BadDamage;
			if (Gauge.Hp < 0) Gauge.Hp = 0;
		}
		else
		{
			++Res.Poor;
			Gauge.Hp -= Gauge.PoorDamage;
			if (Gauge.Hp < 0) Gauge.Hp = 0;
		}
		
		if (Gauge.Type >= GaugeType.Survival && Gauge.Hp <= 0)
		{
			WillSaveData = false;
			UnityEngine.SceneManagement.SceneManager.LoadScene(2);
			Res.Accuracy = AccuracySum / Pat.NoteCount;
			return;
		}
		UI.UpdateScore(Res, Gauge.Hp, AccuracySum / HitCount);
	}

	public void ToggleAuto()
	{
		WillSaveData = false;
		IsAuto = !IsAuto;
	}

	public IEnumerator CheckIfSongEnded()
	{
		while(true)
		{
			if (HitCount >= Pat.NoteCount &&
				((!IsBgaVideoSupported && Pat.BGAChanges.Count == 0) || (IsBgaVideoSupported && !Video.isPlaying)))
			{
				UI.ComboUpTxt("Game Set!");
				Res.Accuracy = AccuracySum / Pat.NoteCount;
				if ((Gauge.Type <= GaugeType.Groove && Gauge.Hp >= 0.8f) || Gauge.Type >= GaugeType.Survival)
					Res.ClearGauge = (int)Gauge.Type;
				else WillSaveData = false;
				break;
			}
			yield return Wait2Sec;
		}

		yield return new WaitWhile(() => Sm.IsSoundPlaying);
		yield return Wait2Sec;
		GameObject.Find("StartPanel").GetComponent<Animator>().Play("FadeInPanel");
		yield return new WaitForSeconds(0.35f);
		UnityEngine.SceneManagement.SceneManager.LoadScene(2);
	}
}