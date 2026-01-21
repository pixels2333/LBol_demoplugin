using System;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 死亡玩家信息条目
/// </summary>
public class DeadPlayerEntry
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public int Level { get; set; }
    public string DeadCause { get; set; }
    public int ResurrectionCost { get; set; }
    public bool CanResurrect { get; set; }
    public int MaxHp { get; set; }
    public DateTime DeathTime { get; set; }
}