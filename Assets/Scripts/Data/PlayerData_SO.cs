using UnityEngine;

/// <summary>
/// 玩家基础数值配置。
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "山海经2026/Player Data")]
public class PlayerData_SO : ScriptableObject
{
    [Header("生命与资源")]
    [SerializeField] int maxHp = 100;
    [SerializeField] int initialCandlelight = 3;
    [SerializeField] int initialStamina = 100;

    [Header("移动与跳跃")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float doubleJumpForce = 8f;

    [Header("受伤")]
    [SerializeField] float hurtInvincibleTime = 1f;
    [SerializeField] float hurtStunTime = 0.3f;

    [Header("攻击")]
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attackKnockback = 5f;

    public int MaxHp => maxHp;
    public int InitialCandlelight => initialCandlelight;
    public int InitialStamina => initialStamina;
    public float MoveSpeed => moveSpeed;
    public float JumpForce => jumpForce;
    public float DoubleJumpForce => doubleJumpForce;
    public float HurtInvincibleTime => hurtInvincibleTime;
    public float HurtStunTime => hurtStunTime;
    public int AttackDamage => attackDamage;
    public float AttackKnockback => attackKnockback;
}
