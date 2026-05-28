using System.Collections.Generic;

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 玩家引导状态，记录中途加入或重连时的初始玩家数据
/// </summary>
public class PlayerBootstrappedState
{
    /// <summary>玩家ID</summary>
    public string PlayerId { get; set; } = string.Empty;
    /// <summary>游戏进度百分比</summary>
    public int GameProgress { get; set; }
    /// <summary>等级</summary>
    public int Level { get; set; }
    /// <summary>当前生命值</summary>
    public int Health { get; set; }
    /// <summary>最大生命值</summary>
    public int MaxHealth { get; set; }
    /// <summary>金币数量</summary>
    public int Gold { get; set; }
    /// <summary>卡牌ID列表</summary>
    public List<string> Cards { get; set; } = [];
    /// <summary>遗物ID列表</summary>
    public List<string> Exhibits { get; set; } = [];
    /// <summary>工具牌ID到数量的映射</summary>
    public Dictionary<string, int> ToolCards { get; set; } = [];
    /// <summary>最后已知事件索引</summary>
    public long LastEventIndex { get; set; }
}
