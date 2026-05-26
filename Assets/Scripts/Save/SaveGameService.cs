using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 存档服务：聚合所有 ISaveData，读写 save.json。
/// </summary>
[DefaultExecutionOrder(-200)]
public class SaveGameService : Singleton<SaveGameService>, ISaveData
{
    const string SaveFileName = "save.json";

    readonly List<ISaveData> _providers = new List<ISaveData>();

    public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    public string SaveGroupKey => "SaveGameService";

    protected override void OnSingletonAwake()
    {
        Register(this);
    }

    public void Register(ISaveData provider)
    {
        if (provider == null || _providers.Contains(provider))
            return;
        _providers.Add(provider);
    }

    public void Unregister(ISaveData provider)
    {
        if (provider == null)
            return;
        _providers.Remove(provider);
    }

    public bool SaveGameToDisk()
    {
        try
        {
            var file = BuildSaveFile();
            var json = JsonUtility.ToJson(file, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[SaveGameService] 存档已写入: {SaveFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveGameService] 存档失败: {ex.Message}");
            return false;
        }
    }

    public bool TryLoadGameFromDisk()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("[SaveGameService] 未找到存档文件。");
            return false;
        }

        try
        {
            var json = File.ReadAllText(SaveFilePath);
            var file = JsonUtility.FromJson<SaveFileData>(json);
            if (file == null)
                return false;

            ApplySaveFile(file);
            Debug.Log($"[SaveGameService] 存档已加载: {SaveFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveGameService] 读档失败: {ex.Message}");
            return false;
        }
    }

    public void DeleteSaveFile()
    {
        if (!File.Exists(SaveFilePath))
            return;

        File.Delete(SaveFilePath);
        Debug.Log("[SaveGameService] 存档已删除。");
    }

    SaveFileData BuildSaveFile()
    {
        var groups = new List<SaveGroupEntry>(_providers.Count);
        foreach (var provider in _providers)
        {
            if (provider == null)
                continue;

            var writer = new SaveGroupWriter();
            provider.WriteSaveData(writer);
            groups.Add(writer.ToEntry(provider.SaveGroupKey));
        }

        return new SaveFileData
        {
            version = SaveFileData.CurrentVersion,
            savedAtUtc = DateTime.UtcNow.ToString("o"),
            groups = groups.ToArray()
        };
    }

    void ApplySaveFile(SaveFileData file)
    {
        if (file?.groups == null)
            return;

        var map = new Dictionary<string, SaveGroupEntry>();
        foreach (var group in file.groups)
        {
            if (group != null && !string.IsNullOrEmpty(group.groupKey))
                map[group.groupKey] = group;
        }

        foreach (var provider in _providers)
        {
            if (provider == null)
                continue;

            if (!map.TryGetValue(provider.SaveGroupKey, out var entry))
                continue;

            provider.ReadSaveData(new SaveGroupReader(entry));
        }
    }

    public void WriteSaveData(SaveGroupWriter writer)
    {
        writer.WriteInt("providerCount", _providers.Count);
    }

    public void ReadSaveData(SaveGroupReader reader)
    {
        // 预留：服务自身元数据
        _ = reader.ReadInt("providerCount", 0);
    }
}
