using System;

namespace NetworkPlugin.UI.Models;

/// <summary>
/// Gap 支援面板中的玩家信息条目。
/// </summary>
public class DeadPlayerEntry
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public int CurrentHp { get; set; }
    public int Level { get; set; }
    public string DeadCause { get; set; }
    public int ResurrectionCost { get; set; }
    public int ActionValue { get; set; }
    public bool CanResurrect { get; set; }
    public int MaxHp { get; set; }
    public string StatusText { get; set; }
    public DateTime DeathTime { get; set; }
}
