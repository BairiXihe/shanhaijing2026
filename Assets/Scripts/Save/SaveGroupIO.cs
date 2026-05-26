using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单个存档分组的写入器（键值均为可 JsonUtility 序列化的基础类型或自定义结构）。
/// </summary>
public sealed class SaveGroupWriter
{
    readonly Dictionary<string, string> _fields = new Dictionary<string, string>();

    public void WriteInt(string key, int value) => _fields[key] = value.ToString();
    public void WriteFloat(string key, float value) => _fields[key] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    public void WriteString(string key, string value) => _fields[key] = value ?? string.Empty;
    public void WriteBool(string key, bool value) => _fields[key] = value ? "1" : "0";

    public void WriteObject<T>(string key, T obj) where T : class
    {
        _fields[key] = obj == null ? string.Empty : JsonUtility.ToJson(obj);
    }

    internal SaveGroupEntry ToEntry(string groupKey) => new SaveGroupEntry
    {
        groupKey = groupKey,
        fields = SaveFieldList.FromDictionary(_fields)
    };
}

/// <summary>
/// 单个存档分组的读取器。
/// </summary>
public sealed class SaveGroupReader
{
    readonly Dictionary<string, string> _fields;

    public SaveGroupReader(SaveGroupEntry entry)
    {
        _fields = entry != null ? entry.ToDictionary() : new Dictionary<string, string>();
    }

    public int ReadInt(string key, int defaultValue = 0)
    {
        if (!_fields.TryGetValue(key, out var raw) || !int.TryParse(raw, out var value))
            return defaultValue;
        return value;
    }

    public float ReadFloat(string key, float defaultValue = 0f)
    {
        if (!_fields.TryGetValue(key, out var raw) ||
            !float.TryParse(raw, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
            return defaultValue;
        return value;
    }

    public string ReadString(string key, string defaultValue = "")
    {
        return _fields.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public bool ReadBool(string key, bool defaultValue = false)
    {
        if (!_fields.TryGetValue(key, out var raw))
            return defaultValue;
        return raw == "1" || string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
    }

    public T ReadObject<T>(string key) where T : class
    {
        if (!_fields.TryGetValue(key, out var json) || string.IsNullOrEmpty(json))
            return null;
        return JsonUtility.FromJson<T>(json);
    }
}

[Serializable]
public class SaveGroupEntry
{
    public string groupKey;
    public SaveFieldList fields;

    public Dictionary<string, string> ToDictionary() =>
        fields != null ? fields.ToDictionary() : new Dictionary<string, string>();
}

[Serializable]
public class SaveFieldList
{
    public string[] keys;
    public string[] values;

    public static SaveFieldList FromDictionary(Dictionary<string, string> dict)
    {
        if (dict == null || dict.Count == 0)
            return new SaveFieldList { keys = Array.Empty<string>(), values = Array.Empty<string>() };

        var keyList = new List<string>(dict.Keys);
        var valueList = new List<string>(keyList.Count);
        foreach (var key in keyList)
            valueList.Add(dict[key]);

        return new SaveFieldList { keys = keyList.ToArray(), values = valueList.ToArray() };
    }

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        if (keys == null || values == null)
            return dict;

        var count = Mathf.Min(keys.Length, values.Length);
        for (var i = 0; i < count; i++)
            dict[keys[i]] = values[i];
        return dict;
    }
}
