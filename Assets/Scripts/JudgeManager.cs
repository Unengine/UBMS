using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JudgeType
{
    IGNORE,
    POOR,
    BAD,
    GOOD,
    GREAT,
    PGREAT
}

public class JudgeManager {

    private static JudgeManager inst = null;
    public static JudgeManager instance
    {
        get
        {
            if (inst == null) inst = new JudgeManager();
            return inst;
        }
        private set { inst = value; }
    }

    public JudgeType Judge(Note n, float currentTime)
    {
        float diff = Mathf.Abs(n.Timing - currentTime) * 1000.0f;
        //Debug.Log($"note : {n.Timing}, currentTime : {currentTime}");
        if (n.Timing > currentTime && diff >= 220.0f) return JudgeType.IGNORE;

        if (diff < 21.0f)
            return JudgeType.PGREAT;
        else if (diff < 60.0f)
            return JudgeType.GREAT;
        else if (diff < 150.0f)
            return JudgeType.GOOD;
        else if (diff < 220.0f)
            return JudgeType.BAD;
        else
            return JudgeType.POOR;

    }
}
