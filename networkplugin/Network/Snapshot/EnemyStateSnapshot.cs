using System.Collections.Generic;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 敌人状态快照
/// </summary>
public class EnemyStateSnapshot
{
    /// <summary>
    /// 敌人ID
    /// </summary>
    public string EnemyId { get; set; } = string.Empty;

    /// <summary>
    /// 敌人名称
    /// </summary>
    public string EnemyName { get; set; } = "";

    /// <summary>
    /// 敌人类型
    /// </summary>
    public string EnemyType { get; set; } = "Normal";

    /// <summary>
    /// 当前生命值
    /// </summary>
    public int Health { get; set; } = 0;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public int MaxHealth { get; set; } = 0;

    /// <summary>
    /// 格挡
    /// </summary>
    public int Block { get; set; } = 0;

    /// <summary>
    /// 护盾
    /// </summary>
    public int Shield { get; set; } = 0;

    /// <summary>
    /// 当前状态效果
    /// </summary>
    public List<StatusEffectStateSnapshot> StatusEffects { get; set; } = [];

    /// <summary>
    /// 当前意图
    /// </summary>
    public IntentionSnapshot Intention { get; set; } = new();

    /// <summary>
    /// 敌人索引（在战场中的位置）
    /// </summary>
    public int Index { get; set; } = 0;

    /// <summary>
    /// 是否存活
    /// </summary>
    public bool IsAlive { get; set; } = true;
}
