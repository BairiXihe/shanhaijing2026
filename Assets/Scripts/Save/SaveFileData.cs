using System;

/// <summary>
/// 磁盘存档根结构（JsonUtility 可序列化）。
/// </summary>
[Serializable]
public class SaveFileData
{
    public const int CurrentVersion = 1;

    public int version;
    public string savedAtUtc;
    public SaveGroupEntry[] groups;
}
