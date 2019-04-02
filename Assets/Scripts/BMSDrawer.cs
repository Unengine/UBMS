using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMSDrawer : MonoBehaviour {
	
    public BMSGameManager Gm;
    public BMSPattern Pat;
    public GameObject NotePrefab;
    public GameObject LongNotePrefab;
    public Transform NoteParent;
    private float[] xPoses;
	private int drawIdx = 0;

    [SerializeField]
    Sprite OddNote;
    [SerializeField]
    Sprite EvenNote;
	[SerializeField]
	Sprite LandMine;
    [SerializeField]
    Sprite LongOddNote;
    [SerializeField]
    Sprite LongEvenNote;
	[SerializeField]
	Sprite ScratchNote;
	[SerializeField]
	Sprite ScratchLongNote;
	[SerializeField]
    GameObject LinePrefab;
	[SerializeField]
	Material Mat;


    // Use this for initialization
    void Init()
	{
		Pat = BMSParser.Instance.Pat;
		xPoses = new float[9];
		//xPoses[0] = -2.125f; 0.875
		xPoses[0] = -10.81667f;
		xPoses[1] = -9.94167f;
		xPoses[2] = -9.06667f;
		xPoses[3] = -8.19167f;
		xPoses[4] = -7.31667f;
		xPoses[5] = -11.69167f;
		xPoses[6] = -10; //페달, 지원 안함
        xPoses[7] = -6.44167f;
		xPoses[8] = -5.56667f;

		//6.44167 -12.56667f;
        
    }

    public void DrawNotes()
    {
		Init();

		for (int i = 0; i < 9; ++i)
		{
			Vector3 prev = Vector2.zero;
			for (int j = Pat.Lines[i].NoteList.Count - 1; j >= 0; --j)
			{
				Note n = Pat.Lines[i].NoteList[j];
				GameObject note = Instantiate(NotePrefab, NoteParent) as GameObject;
				if (i == 5) note.GetComponent<SpriteRenderer>().sprite = ScratchNote;
				else if ((i & 1) == 0) note.GetComponent<SpriteRenderer>().sprite = OddNote;
				else note.GetComponent<SpriteRenderer>().sprite = EvenNote;

				note.transform.position = new Vector2(xPoses[i], (float)(n.Beat * BMSGameManager.Speed));
				if (n.Extra == 1)
				{
					GameObject longNote = Instantiate(LongNotePrefab, NoteParent) as GameObject;
					if (i == 5) longNote.GetComponent<SpriteRenderer>().sprite = ScratchLongNote;
					else if ((i & 1) == 0) longNote.GetComponent<SpriteRenderer>().sprite = LongOddNote;
					else longNote.GetComponent<SpriteRenderer>().sprite = LongEvenNote;
					longNote.transform.position = (note.transform.position + prev) * 0.5f + Vector3.up * 0.1875f;
					longNote.transform.localScale = new Vector3(1.0f, (note.transform.position - prev).y * 2.666666f, 1.0f);
				}
				prev = note.transform.position;
				n.Model = note;
			}
		}

        for (int i = 0; i < 9; ++i)
        {
            for (int j = Pat.Lines[i].LandMineList.Count - 1; j >= 0; --j)
            {
                Note n = Pat.Lines[i].LandMineList[j];
                GameObject note = Instantiate(NotePrefab, NoteParent) as GameObject;
                note.GetComponent<SpriteRenderer>().sprite = LandMine;
                note.transform.position = new Vector2(xPoses[i], (float)(n.Beat * BMSGameManager.Speed));
                n.Model = note;
            }
        }
	}

	void OnRenderObject()
	{
		if (BMSGameManager.IsPaused) return;

		if (!Mat)
		{
			Debug.LogError("BMSDrawer has no material!");
			return;
		}

		GL.PushMatrix();
		Mat.SetPass(0);

		for (int i = drawIdx; i < Pat.BarCount; ++i)
		{
			float y = (float)(Pat.GetPreviousBarBeatSum(i) * BMSGameManager.Speed - Gm.Scroll);
			if (y < 0.25f)
			{
				drawIdx = i - 1;
				continue;
			}
			if (y > 12.0f) break;

			GL.Begin(GL.LINES);
			GL.Color(Color.white);

			GL.Vertex(new Vector3(-12.12917f, y, 0.0f));
			GL.Vertex(new Vector3(-5.129167f, y, 0.0f));

			GL.End();
		}

		GL.PopMatrix();
	}
}
