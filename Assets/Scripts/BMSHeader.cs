using System;
using System.Collections;
using System.Collections.Generic;

public class BMSSongInfo
{
	public List<BMSHeader> Headers;
	public string SongName;
	public BMSSongInfo()
	{
		Headers = new List<BMSHeader>() { Capacity = 4 };
	}

}

public class BMSHeader
{
	public string ParentPath { get; set; }
    [Flags]
    public enum Lntype { NONE = 0, LN1 = 1 << 1, LN2 = 1 << 2, LNOBJ = 1 << 3}

    public int Level { get; set; }
    public int Player { get; set; }
	public string BGImagePath { get; set; }
    public string Artist { get; set; }
    public string Genre { get; set; }
    public string Title { get; set; }
    public double Total { get; set; }
    public double Bpm { get; set; }
    public int Rank { get; set; }
    public Lntype LnType { get; set; }
    public int Lnobj { get; set; }
}
