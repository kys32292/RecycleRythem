using System;
using System.Collections.Generic;

[Serializable]
public class ChartNoteData
{
    public float time;
    public LaneType lane;
}

[Serializable]
public class ChartFileData
{
    public string songName;
    public float songOffset;
    public List<ChartNoteData> notes = new List<ChartNoteData>();
}

public enum LaneType
{
    Upper = 0,
    Lower = 1
}
