using System;
using UnityEngine;

/// <summary>
/// 游戏全局管理：状态、暂停、重试、主菜单。
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : Singleton<GameManager>, ISaveData
{
    [SerializeField] string mainMenuSceneName = "";
    [SerializeField] bool autoLoadSaveOnStart;

    GameState _currentState = GameState.Menu;
    float _timeScaleBeforePause = 1f;

    public string SaveGroupKey => "GameManager";

    public GameState CurrentState => _currentState;

    protected override void OnSingletonAwake()
    {
        InitializeManagers();
        SaveGameService.Instance.Register(this);

        if (autoLoadSaveOnStart)
            SaveGameService.Instance.TryLoadGameFromDisk();
    }

    protected override void OnDestroy()
    {
        if (SaveGameService.HasInstance)
            SaveGameService.Instance.Unregister(this);
        base.OnDestroy();
    }

    void InitializeManagers()
    {
        _ = SaveGameService.Instance;
        _ = ConfigManager.Instance;
        _ = LevelManager.Instance;
    }

    public void SetState(GameState state)
    {
        _currentState = state;
    }

    public void PauseGame()
    {
        if (_currentState != GameState.Playing)
            return;

        _timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (_currentState != GameState.Paused)
            return;

        Time.timeScale = _timeScaleBeforePause > 0f ? _timeScaleBeforePause : 1f;
        SetState(GameState.Playing);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SetState(GameState.Menu);

        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManagementHelper.LoadScene(mainMenuSceneName);
    }

    public void RetryCurrentLevel()
    {
        Time.timeScale = 1f;
        SetState(GameState.Playing);

        if (LevelManager.HasInstance)
            LevelManager.Instance.RestartCurrentLevel();
    }

    public void StartLevel(int levelIndex)
    {
        Time.timeScale = 1f;
        SetState(GameState.Playing);
        LevelManager.Instance.LoadLevel(levelIndex);
    }

    public void SaveGame()
    {
        SaveGameService.Instance.SaveGameToDisk();
    }

    public void WriteSaveData(SaveGroupWriter writer)
    {
        writer.WriteInt("gameState", (int)_currentState);
    }

    public void ReadSaveData(SaveGroupReader reader)
    {
        var stateValue = reader.ReadInt("gameState", (int)GameState.Menu);
        if (Enum.IsDefined(typeof(GameState), stateValue))
            _currentState = (GameState)stateValue;
    }

    static class SceneManagementHelper
    {
        public static void LoadScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
