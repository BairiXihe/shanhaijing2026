using UnityEngine;

/// <summary>
/// 泛型单例基类：自动查找或创建实例，Awake 中保证唯一。
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            _instance = FindObjectOfType<T>();
            if (_instance != null)
                return _instance;

            var go = new GameObject(typeof(T).Name);
            _instance = go.AddComponent<T>();
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = (T)this;
        DontDestroyOnLoad(gameObject);
        OnSingletonAwake();
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>子类可重写以在单例确立后执行初始化。</summary>
    protected virtual void OnSingletonAwake() { }
}
