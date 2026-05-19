namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 玩家身份接口——包含唯一标识和显示名称
/// </summary>
public interface IPlayerIdentity
{
    /// <summary>服务器分配的玩家唯一标识</summary>
    string playerId { get; set; }

    /// <summary>玩家用户名</summary>
    string userName { get; set; }

    /// <summary>玩家角色标识</summary>
    string chara { get; set; }

    /// <summary>判断玩家是否为房主</summary>
    bool IsLobbyOwner();
}
