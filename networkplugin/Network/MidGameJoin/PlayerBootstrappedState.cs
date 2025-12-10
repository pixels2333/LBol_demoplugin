using System.Collections.Generic;

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 玩家引导状态
/// </summary>
public class PlayerBootstrappedState
{
    public string PlayerId { get; set; } = string.Empty;
    public int GameProgress { get; set; }
    public int Level { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Gold { get; set; }
    public List<string> Cards { get; set; } = [];
    public List<string> Exhibits { get; set; } = [];
    public Dictionary<string, int> Potions { get; set; } = [];
    public long LastEventIndex { get; set; }
}
