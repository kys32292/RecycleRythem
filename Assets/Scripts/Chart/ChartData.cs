using System;
using System.Collections.Generic;

[Serializable]
public class ChartNoteData
{
    public float time;
    // 리믹스에서 사용할 경로 번호입니다. 기존 채보에 없으면 0이며 일반 모드에서는 무시합니다.
    public int lane;
}

[Serializable]
public class ChartFileData
{
    public string songName;
    public float songOffset;
    public List<ChartNoteData> notes = new List<ChartNoteData>();
}
