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
    GameObject LinePrefab;

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

                note.transform.position = new Vector2(xPoses[i], (float)(n.Beat * BMSGameManager.speed));
                if (n.Extra == 1)
                {
                    GameObject longNote = Instantiate(longNotePrefab, noteParent) as GameObject;
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

        for (int i = 0; i < pat.BeatCTable.Count; ++i)
        {
            GameObject inst = Instantiate(LinePrefab, noteParent) as GameObject;
            inst.transform.position = new Vector2(0, (float)(pat.GetPreviousBarBeatSum(i) * BMSGameManager.speed));
        }
    }
}
