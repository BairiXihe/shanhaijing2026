using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 关卡场景加载与通关条件检查（预留）。
/// </summary>
[DefaultExecutionOrder(-90)]
public class LevelManager : Singleton<LevelManager>, ISaveData
{
    static readonly string[] LevelSceneNames =
    {
        "第一关",
        "第二关",
        "第三关",
        "第四关",
        "第五关",
        "第六关"
    };

    [SerializeField] string loadingHintText = "加载中...";

    int _currentLevelIndex;
    Vector3 _playerSpawnPosition;
    bool _isLoading;
    string _loadingMessage;

    public string SaveGroupKey => "LevelManager";

    public int CurrentLevelIndex => _currentLevelIndex;
    public int LevelCount => LevelSceneNames.Length;
    public Vector3 PlayerSpawnPosition => _playerSpawnPosition;
    public bool IsLoading => _isLoading;

    protected override void OnSingletonAwake()
    {
        SaveGameService.Instance.Register(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (SaveGameService.HasInstance)
            SaveGameService.Instance.Unregister(this);
        base.OnDestroy();
    }

    void OnGUI()
    {
        if (!_isLoading)
            return;

        var rect = new Rect(0f, 0f, Screen.width, Screen.height);
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 28
        };
        GUI.Label(rect, string.IsNullOrEmpty(_loadingMessage) ? loadingHintText : _loadingMessage, style);
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= LevelSceneNames.Length)
        {
            Debug.LogWarning($"[LevelManager] 无效关卡索引: {index}");
            return;
        }

        _currentLevelIndex = index;
        var sceneName = LevelSceneNames[index];
        var levelConfig = ConfigManager.HasInstance
            ? ConfigManager.Instance.GetLevelDataByIndex(index)
            : null;

        if (levelConfig != null)
            _playerSpawnPosition = levelConfig.SpawnPosition;

        StartCoroutine(LoadSceneWithHint(sceneName, $"正在进入 {sceneName}..."));
    }

    public void LoadNextLevel()
    {
        if (_currentLevelIndex + 1 >= LevelSceneNames.Length)
        {
            Debug.Log("[LevelManager] 已是最后一关。");
            if (GameManager.HasInstance)
                GameManager.Instance.SetState(GameState.LevelComplete);
            return;
        }

        LoadLevel(_currentLevelIndex + 1);
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(_currentLevelIndex);
    }

    public LevelObjective GetCurrentLevelObjective()
    {
        var data = ConfigManager.HasInstance
            ? ConfigManager.Instance.GetLevelDataByIndex(_currentLevelIndex)
            : null;
        return data != null ? data.Objective : LevelObjective.ReachEnd;
    }

    /// <summary>
    /// 检查当前关卡是否通关（预留接口，后续接入终点/Boss 等）。
    /// </summary>
    public bool CheckLevelComplete()
    {
        // TODO: 根据 GetCurrentLevelObjective() 接入具体通关逻辑
        return false;
    }

    public void SetPlayerSpawnPosition(Vector3 position)
    {
        _playerSpawnPosition = position;
    }

    IEnumerator LoadSceneWithHint(string sceneName, string hint)
    {
        _loadingMessage = hint;
        _isLoading = true;
        yield return null;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[LevelManager] 场景未加入 Build Settings 或名称错误: {sceneName}");
            _isLoading = false;
            _loadingMessage = null;
            yield break;
        }

        SceneManager.LoadScene(sceneName);
        _isLoading = false;
        _loadingMessage = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyPlayerSpawnPosition();
        SyncLevelIndexFromSceneName(scene.name);

        if (GameManager.HasInstance)
            GameManager.Instance.SetState(GameState.Playing);
    }

    void SyncLevelIndexFromSceneName(string sceneName)
    {
        for (var i = 0; i < LevelSceneNames.Length; i++)
        {
            if (LevelSceneNames[i] == sceneName)
            {
                _currentLevelIndex = i;
                return;
            }
        }
    }

    void ApplyPlayerSpawnPosition()
    {
        var player = FindObjectOfType<PlayerController>();
        if (player == null)
            return;

        player.transform.position = _playerSpawnPosition;
    }

    public void WriteSaveData(SaveGroupWriter writer)
    {
        writer.WriteInt("currentLevelIndex", _currentLevelIndex);
        writer.WriteFloat("spawnX", _playerSpawnPosition.x);
        writer.WriteFloat("spawnY", _playerSpawnPosition.y);
        writer.WriteFloat("spawnZ", _playerSpawnPosition.z);
    }

    public void ReadSaveData(SaveGroupReader reader)
    {
        _currentLevelIndex = Mathf.Clamp(reader.ReadInt("currentLevelIndex", 0), 0, LevelSceneNames.Length - 1);
        _playerSpawnPosition = new Vector3(
            reader.ReadFloat("spawnX", 0f),
            reader.ReadFloat("spawnY", 0f),
            reader.ReadFloat("spawnZ", 0f));
    }

}
