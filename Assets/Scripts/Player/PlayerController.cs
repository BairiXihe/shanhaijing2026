using UnityEngine;

/// <summary>
/// 2D 横板玩家控制器：移动、二段跳、地面检测、朝向翻转。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    const int MaxJumps = 2;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float doubleJumpForce = 8f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Vector2 groundCheckSize = new Vector2(0.45f, 0.08f);
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D _rigidbody;
    bool _isGrounded;
    int _jumpsRemaining;
    float _facingDirection = 1f;

    // 来自配表的运行时数值（供战斗等系统读取）
    int _maxHp;
    int _attackDamage;
    float _attackKnockback;

    public int MaxHp => _maxHp;
    public int AttackDamage => _attackDamage;
    public float AttackKnockback => _attackKnockback;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.freezeRotation = true;

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    void Start()
    {
        ApplyPlayerConfig();
    }

    void ApplyPlayerConfig()
    {
        if (!ConfigManager.HasInstance)
            return;

        var data = ConfigManager.Instance.GetPlayerData();
        if (data == null)
            return;

        moveSpeed = data.MoveSpeed;
        jumpForce = data.JumpForce;
        doubleJumpForce = data.DoubleJumpForce;
        _maxHp = data.MaxHp;
        _attackDamage = data.AttackDamage;
        _attackKnockback = data.AttackKnockback;
        _hurtInvincibleTime = data.HurtInvincibleTime;
        _hurtStunTime = data.HurtStunTime;
    }

    void Update()
    {
        UpdateGroundState();
        HandleJumpInput();
        UpdateFacing();
    }

    void FixedUpdate()
    {
        ApplyHorizontalMovement();
    }

    void UpdateGroundState()
    {
        Vector2 checkPosition = groundCheck != null
            ? groundCheck.position
            : (Vector2)transform.position + Vector2.down * 0.55f;

        _isGrounded = Physics2D.OverlapBox(checkPosition, groundCheckSize, 0f, groundLayer);

        if (_isGrounded)
            _jumpsRemaining = MaxJumps;
    }

    void ApplyHorizontalMovement()
    {
        float moveInput = InputManager.Instance != null ? InputManager.Instance.MoveInput : 0f;
        Vector2 velocity = _rigidbody.velocity;
        velocity.x = moveInput * moveSpeed;
        _rigidbody.velocity = velocity;
    }

    void HandleJumpInput()
    {
        if (InputManager.Instance == null || !InputManager.Instance.JumpPressed)
            return;

        if (_isGrounded)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpForce);
            _jumpsRemaining = MaxJumps - 1;
            return;
        }

        if (_jumpsRemaining <= 0)
            return;

        _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, doubleJumpForce);
        _jumpsRemaining--;
    }

    void UpdateFacing()
    {
        if (InputManager.Instance == null)
            return;

        float moveInput = InputManager.Instance.MoveInput;
        if (moveInput > 0.01f)
            _facingDirection = 1f;
        else if (moveInput < -0.01f)
            _facingDirection = -1f;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _facingDirection;
        transform.localScale = scale;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Vector2 checkPosition = groundCheck != null
            ? groundCheck.position
            : (Vector2)transform.position + Vector2.down * 0.55f;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireCube(checkPosition, groundCheckSize);
    }
#endif
}
