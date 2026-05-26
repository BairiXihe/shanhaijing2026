using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 配置表管理器：启动时从 Resources 加载 ScriptableObject。
/// </summary>
[DefaultExecutionOrder(-150)]
public class ConfigManager : Singleton<ConfigManager>
{
    const string PlayerDataPath = "Config/PlayerData";
    const string LevelDataPath = "Config/LevelData";

    readonly Dictionary<int, LevelData_SO> _levelDataById = new Dictionary<int, LevelData_SO>();

    PlayerData_SO _playerData;

    public PlayerData_SO PlayerData => _playerData;

    protected override void OnSingletonAwake()
    {
        LoadAllConfigs();
    }

    void LoadAllConfigs()
    {
        var playerAssets = Resources.LoadAll<PlayerData_SO>(PlayerDataPath);
        _playerData = playerAssets.Length > 0 ? playerAssets[0] : null;
        if (_playerData == null)
            Debug.LogWarning($"[ConfigManager] 未找到玩家配置: Resources/{PlayerDataPath}");

        _levelDataById.Clear();
        var levelAssets = Resources.LoadAll<LevelData_SO>(LevelDataPath);
        foreach (var level in levelAssets)
        {
            if (level == null)
                continue;

            if (_levelDataById.ContainsKey(level.LevelId))
                Debug.LogWarning($"[ConfigManager] 重复的关卡 ID: {level.LevelId} ({level.name})");
            else
                _levelDataById.Add(level.LevelId, level);
        }

        if (_playerData == null || _levelDataById.Count == 0)
            CreateRuntimeFallbackConfigs();

        Debug.Log($"[ConfigManager] 已加载玩家配置 {(_playerData != null ? 1 : 0)} 条，关卡配置 {_levelDataById.Count} 条。");
    }

    void CreateRuntimeFallbackConfigs()
    {
        if (_playerData == null)
        {
            _playerData = ScriptableObject.CreateInstance<PlayerData_SO>();
            Debug.LogWarning("[ConfigManager] 使用运行时默认 PlayerData（请在编辑器执行 山海经2026/Create Config Assets）。");
        }

        if (_levelDataById.Count > 0)
            return;

        var scenes = new[]
        {
            "第一关", "第二关", "第三关", "第四关", "第五关", "第六关"
        };

        for (var i = 0; i < scenes.Length; i++)
        {
            var level = ScriptableObject.CreateInstance<LevelData_SO>();
            var objective = i == scenes.Length - 1 ? LevelObjective.DefeatBoss : LevelObjective.ReachEnd;
            level.Initialize(i + 1, scenes[i], scenes[i], objective, new Vector3(0f, 1f, 0f));
            _levelDataById.Add(level.LevelId, level);
        }

        Debug.LogWarning("[ConfigManager] 使用运行时默认 LevelData（请在编辑器执行 山海经2026/Create Config Assets）。");
    }

    public PlayerData_SO GetPlayerData() => _playerData;

    public LevelData_SO GetLevelData(int levelId)
    {
        _levelDataById.TryGetValue(levelId, out var data);
        return data;
    }

    public LevelData_SO GetLevelDataByIndex(int levelIndex)
    {
        return GetLevelData(levelIndex + 1);
    }

    public IReadOnlyCollection<LevelData_SO> GetAllLevelData() => _levelDataById.Values;
}
