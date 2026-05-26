using UnityEngine;

/// <summary>
/// 关卡配置：场景、目标、出生点等。
/// </summary>
[CreateAssetMenu(fileName = "LevelData", menuName = "山海经2026/Level Data")]
public class LevelData_SO : ScriptableObject
{
    [SerializeField] int levelId = 1;
    [SerializeField] string levelName = "第一关";
    [SerializeField] string sceneName = "第一关";
    [SerializeField] LevelObjective objective = LevelObjective.ReachEnd;

    [Tooltip("玩家出生位置；也可用 SpawnPositionText 填写 \"x y z\"")]
    [SerializeField] Vector3 spawnPosition;

    [Tooltip("可选：\"x y z\" 格式，非空时覆盖 Spawn Position")]
    [SerializeField] string spawnPositionText;

    public int LevelId => levelId;
    public string LevelName => levelName;
    public string SceneName => sceneName;
    public LevelObjective Objective => objective;

    /// <summary>运行时填充默认配置（无 Resources 资源时使用）。</summary>
    public void Initialize(int id, string displayName, string scene, LevelObjective levelObjective, Vector3 spawn)
    {
        levelId = id;
        levelName = displayName;
        sceneName = scene;
        objective = levelObjective;
        spawnPosition = spawn;
        spawnPositionText = string.Empty;
    }

    public Vector3 SpawnPosition
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(spawnPositionText) &&
                TryParseVector3(spawnPositionText, out var parsed))
                return parsed;
            return spawnPosition;
        }
    }

    static bool TryParseVector3(string text, out Vector3 result)
    {
        result = Vector3.zero;
        var parts = text.Trim().Split(' ');
        if (parts.Length < 3)
            return false;

        if (!float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var x))
            return false;
        if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var y))
            return false;
        if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var z))
            return false;

        result = new Vector3(x, y, z);
        return true;
    }
}
