using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 玩家网络同步接口——所有更新/同步方法
/// </summary>
public interface IPlayerNetworkSync
{
    /// <summary>发送玩家数据到网络</summary>
    void SendData();

    /// <summary>存档加载后执行的操作</summary>
    void PostSaveLoad();

    /// <summary>更新生命值</summary>
    void UpdateHealth(bool updateServer);

    /// <summary>更新格挡值</summary>
    void UpdateBlock(bool updateServer);

    /// <summary>更新最大生命值</summary>
    void UpdateMaxHP(bool updateServer);

    /// <summary>更新金币数量</summary>
    void UpdateCoins(bool updateServer);

    /// <summary>更新玩家基本信息</summary>
    void UpdatePlayerInfo(bool updateServer);

    /// <summary>更新心境</summary>
    void UpdateMood(bool updateServer);

    /// <summary>更新状态效果</summary>
    void UpdateStatusEffects(bool updateServer);

    /// <summary>更新终极能量</summary>
    void UpdateUltimatePower(bool updateServer);

    /// <summary>更新展品</summary>
    void UpdateExhibits(bool updateServer);

    /// <summary>更新法力值</summary>
    void UpdateMana(bool updateServer);

    /// <summary>更新结束回合状态</summary>
    void UpdateEndTurn(bool updateServer);

    /// <summary>更新位置信息</summary>
    void UpdateLocation(MapNode visitingnode, bool updateServer = true);

    /// <summary>更新存活状态</summary>
    void UpdateLiveStatus(bool updateServer);
}
