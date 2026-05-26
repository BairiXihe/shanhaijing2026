#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 一键生成 Resources/Config 下的示例配表资源。
/// </summary>
public static class ConfigAssetCreator
{
    const string PlayerFolder = "Assets/Resources/Config/PlayerData";
    const string LevelFolder = "Assets/Resources/Config/LevelData";

    static readonly string[] LevelScenePaths =
    {
        "Assets/Scenes/第一关.unity",
        "Assets/Scenes/第二关.unity",
        "Assets/Scenes/第三关.unity",
        "Assets/Scenes/第四关.unity",
        "Assets/Scenes/第五关.unity",
        "Assets/Scenes/第六关.unity"
    };

    static readonly (int id, string name, string scene, LevelObjective objective)[] LevelDefinitions =
    {
        (1, "第一关", "第一关", LevelObjective.ReachEnd),
        (2, "第二关", "第二关", LevelObjective.ReachEnd),
        (3, "第三关", "第三关", LevelObjective.ReachEnd),
        (4, "第四关", "第四关", LevelObjective.ReachEnd),
        (5, "第五关", "第五关", LevelObjective.ReachEnd),
        (6, "第六关", "第六关", LevelObjective.DefeatBoss)
    };

    [InitializeOnLoadMethod]
    static void AutoCreateOnProjectLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorPrefs.GetBool("Shanhaijing_ConfigAssets_v1", false))
                return;

            CreateAllConfigAssets();
            EditorPrefs.SetBool("Shanhaijing_ConfigAssets_v1", true);
        };
    }

    [MenuItem("山海经2026/Create Config Assets")]
    public static void CreateAllConfigAssets()
    {
        EnsureFolder(PlayerFolder);
        EnsureFolder(LevelFolder);

        CreateDefaultPlayerData();
        CreateLevelDataAssets();
        EnsureLevelScenesInBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[山海经2026] 配置资源已生成到 Resources/Config/。");
    }

    static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
            return;

        var parts = assetPath.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static void CreateDefaultPlayerData()
    {
        var assetPath = $"{PlayerFolder}/DefaultPlayerData.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PlayerData_SO>(assetPath);
        if (existing != null)
        {
            Debug.Log($"[ConfigAssetCreator] 已存在: {assetPath}");
            return;
        }

        var asset = ScriptableObject.CreateInstance<PlayerData_SO>();
        AssetDatabase.CreateAsset(asset, assetPath);
    }

    static void CreateLevelDataAssets()
    {
        foreach (var def in LevelDefinitions)
        {
            var assetPath = $"{LevelFolder}/Level_{def.id:D2}_{def.scene}.asset";
            if (AssetDatabase.LoadAssetAtPath<LevelData_SO>(assetPath) != null)
            {
                Debug.Log($"[ConfigAssetCreator] 已存在: {assetPath}");
                continue;
            }

            var asset = ScriptableObject.CreateInstance<LevelData_SO>();
            var so = new SerializedObject(asset);
            so.FindProperty("levelId").intValue = def.id;
            so.FindProperty("levelName").stringValue = def.name;
            so.FindProperty("sceneName").stringValue = def.scene;
            so.FindProperty("objective").enumValueIndex = (int)def.objective;
            so.FindProperty("spawnPosition").vector3Value = new Vector3(0f, 1f, 0f);
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(asset, assetPath);
        }
    }

    static void EnsureLevelScenesInBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        foreach (var path in LevelScenePaths)
        {
            if (!File.Exists(path))
                continue;

            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        if (scenes.Count == 0)
            return;

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[ConfigAssetCreator] Build Settings 已注册 {scenes.Count} 个关卡场景。");
    }
}
#endif
