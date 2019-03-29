using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BMSDrawer : MonoBehaviour {

    public BMSHeader header;
    public BMSPattern pat;
    public GameObject notePrefab;
    public GameObject longNotePrefab;
    public Transform noteParent;
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
	Material mat;


    // Use this for initialization
    void Awake () {
        header = BMSParser.instance.header;
        pat = BMSParser.instance.pat;
        xPoses = new float[9];
        xPoses[0] = -2.125f;
        xPoses[1] = -1.25f;
        xPoses[2] = -0.375f;
        xPoses[3] = 0.5f;
        xPoses[4] = 1.375f;
        xPoses[5] = -3f;
        xPoses[6] = -10; //무쓸모
        xPoses[7] = 2.25f;
        xPoses[8] = 3.125f;
        
    }

    public void DrawNotes()
    {
		Debug.Log("Draw");
		for (int i = 0; i < 9; ++i)
		{
			Vector3 prev = Vector2.zero;
			for (int j = pat.Lines[i].noteList.Count - 1; j >= 0; --j)
			{
				Note n = pat.Lines[i].noteList[j];
				GameObject note = Instantiate(notePrefab, noteParent) as GameObject;
				if (i == 5) note.GetComponent<SpriteRenderer>().sprite = ScratchNote;
				else if ((i & 1) == 0) note.GetComponent<SpriteRenderer>().sprite = OddNote;
				else note.GetComponent<SpriteRenderer>().sprite = EvenNote;


				note.transform.position = new Vector2(xPoses[i], (float)(n.Beat * BMSGameManager.speed));
				if (n.Extra == 1)
				{
					GameObject longNote = Instantiate(longNotePrefab, noteParent) as GameObject;
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
            for (int j = pat.Lines[i].landMineList.Count - 1; j >= 0; --j)
            {
                Note n = pat.Lines[i].landMineList[j];
                GameObject note = Instantiate(notePrefab, noteParent) as GameObject;
                note.GetComponent<SpriteRenderer>().sprite = LandMine;
                note.transform.position = new Vector2(xPoses[i], (float)(n.Beat * BMSGameManager.speed));
                n.Model = note;
            }
        }
	}

	void OnRenderObject()
	{
		if (!mat)
		{
			Debug.LogError("BMSDrawer has no material!");
			return;
		}

		GL.PushMatrix();
		mat.SetPass(0);

		for (int i = drawIdx; i < pat.BarCount; ++i)
		{
			float y = (float)(pat.GetPreviousBarBeatSum(i) * BMSGameManager.speed - BMSGameManager.scroll);
			if (y < 0.25f)
			{
				drawIdx = i - 1;
				continue;
			}
			if (y > 12.0f) break;

			GL.Begin(GL.LINES);
			GL.Color(Color.white);

			GL.Vertex(new Vector3(-3.4375f, y, 0.0f));
			GL.Vertex(new Vector3(3.5625f, y, 0.0f));

			GL.End();
		}

		GL.PopMatrix();
	}
}
