using UnityEngine;

/// <summary>
/// 统一管理玩家输入，供 PlayerController 等系统读取。
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    PlayerInputActions _inputActions;

    /// <summary>水平移动输入，范围约 -1 ~ 1。</summary>
    public float MoveInput { get; private set; }

    /// <summary>本帧是否按下跳跃键（仅按下瞬间为 true，防止连跳）。</summary>
    public bool JumpPressed { get; private set; }

    /// <summary>本帧是否按下攻击键。</summary>
    public bool AttackPressed { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        _inputActions?.Disable();
        _inputActions?.Dispose();
    }

    void Update()
    {
        MoveInput = _inputActions.Player.Move.ReadValue<float>();
        JumpPressed = _inputActions.Player.Jump.WasPressedThisFrame();
        AttackPressed = _inputActions.Player.Attack.WasPressedThisFrame();
    }
}
