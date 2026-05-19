namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 玩家资源接口——金币、展品、法力值、交易状态、终极能量
/// </summary>
public interface IPlayerResources
{
    /// <summary>金币数量</summary>
    int coins { get; set; }

    /// <summary>展品列表</summary>
    string[] exhibits { get; set; }

    /// <summary>交易状态</summary>
    bool tradingStatus { get; set; }

    /// <summary>终极能量</summary>
    bool ultimatePower { get; set; }

    /// <summary>法力值数组</summary>
    int[] mana { get; set; }

    /// <summary>姿态（兼容 mood）</summary>
    string stance { get; set; }
}
