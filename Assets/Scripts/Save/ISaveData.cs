/// <summary>
/// 可参与存档的模块接口，由 SaveGameService 聚合读写。
/// </summary>
public interface ISaveData
{
    /// <summary>存档分组键，需全局唯一。</summary>
    string SaveGroupKey { get; }

    void WriteSaveData(SaveGroupWriter writer);
    void ReadSaveData(SaveGroupReader reader);
}
