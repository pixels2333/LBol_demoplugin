
using System;
using LBoL.Core;
using UnityEngine.InputSystem.EnhancedTouch;

namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 定义网络玩家的基本属性和操作接口。
/// </summary>
public interface INetworkPlayer
{
    /// <summary>
    /// 玩家用户名。
    /// </summary>
    string username { get; set; }

    /// <summary>
    /// 玩家当前生命值。
    /// </summary>
    int HP { get; set; }

    /// <summary>
    /// 玩家最大生命值。
    /// </summary>
    int maxHP { get; set; }

    /// <summary>
    /// 玩家当前格挡值。
    /// </summary>
    int block { get; set; }

    /// <summary>
    /// 玩家当前护盾值。
    /// </summary>
    int shield { get; set; }

    /// <summary>
    /// 玩家金币数量。
    /// </summary>
    int coins { get; set; }

    /// <summary>
    /// 玩家角色标识。
    /// </summary>
    string chara { get; set; }

    /// <summary>
    /// 玩家终极能量。
    /// </summary>
    int UltimatePower { get; set; }

    /// <summary>
    /// 玩家当前位置描述。
    /// </summary>
    string location { get; set; }

    /// <summary>
    /// 是否已结束回合。
    /// </summary>
    bool endturn { get; set; }

    /// <summary>
    /// 玩家法力数组。
    /// </summary>
    int[] mana { get; set; }

    /// <summary>
    /// 玩家当前姿态。
    /// </summary>
    string stance { get; set; }

    /// <summary>
    /// 玩家持有的展品列表。
    /// </summary>
    string[] exhibits { get; set; }

    /// <summary>
    /// 玩家交易状态。
    /// </summary>
    bool tradingStatus { get; set; }

    /// <summary>
    /// 玩家是否处于终极状态（布尔版本）。
    /// </summary>
    bool ultimatePower { get; set; }

    /// <summary>
    /// 玩家所在节点X坐标。
    /// </summary>
    int location_X { get; set; }

    /// <summary>
    /// 玩家所在节点Y坐标。
    /// </summary>
    int location_Y { get; set; }

    /// <summary>
    /// 发送玩家数据到网络。
    /// </summary>
    void SendData();

    /// <summary>
    /// 判断玩家是否为房主。
    /// </summary>
    /// <returns>如果是房主返回true，否则返回false。</returns>
    bool IsLobbyOwner();

    /// <summary>
    /// 存档加载后执行的操作。
    /// </summary>
    void PostSaveLoad();

    /// <summary>
    /// 判断玩家是否在同一房间。
    /// </summary>
    /// <returns>如果在同一房间返回true，否则返回false。</returns>
    bool IsPlayerInSameRoom();

    /// <summary>
    /// 判断玩家是否在同一章节。
    /// </summary>
    /// <returns>如果在同一章节返回true，否则返回false。</returns>
    bool IsPlayerOnSameAct();

    /// <summary>
    /// 检测玩家是否濒死，并可选择通知服务器。
    /// </summary>
    /// <param name="updateServer">是否将结果同步至服务器。</param>
    void IsNearDeath(bool updateServer);

    /// <summary>
    /// 判断是否应渲染该玩家的角色模型。
    /// </summary>
    /// <returns>需要渲染时返回 true。</returns>
    bool ShouldRenderCharacter();

    /// <summary>
    /// 判断是否应渲染该玩家的角色信息框。
    /// </summary>
    /// <returns>需要渲染时返回 true。</returns>
    bool ShouldRenderCharacterInfoBox();

    /// <summary>
    /// 更新玩家生命值，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateHealth(bool updateServer);

    /// <summary>
    /// 更新玩家格挡值，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateBlock(bool updateServer);

    /// <summary>
    /// 更新玩家最大生命值，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateMaxHP(bool updateServer);

    /// <summary>
    /// 更新玩家金币数量，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateCoins(bool updateServer);

    /// <summary>
    /// 更新玩家综合信息（含HP、格挡等），并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdatePlayerInfo(bool updateServer);

    //TODO:stance名称可能要改
    /// <summary>
    /// 更新玩家当前姿态，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateStance(bool updateServer);

    // void ClearPowers(bool updateServer);

    // void UpdatePowers(bool updateServer);

    // void UpdateTempPowers(bool updateServer);
    /// <summary>
    /// 更新玩家状态效果列表，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateStatusEffects(bool updateServer);

    /// <summary>
    /// 更新玩家终极能量值，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateUltimatePower(bool updateServer);

    /// <summary>
    /// 更新玩家持有的展品列表，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateExhibits(bool updateServer);

    /// <summary>
    /// 更新玩家法力值，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateMana(bool updateServer);

    /// <summary>
    /// 更新玩家回合结束状态，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateEndTurn(bool updateServer);

    /// <summary>
    /// 更新玩家当前地图节点位置，并可选择同步至服务器。
    /// </summary>
    /// <param name="visitingnode">玩家正在访问的地图节点。</param>
    /// <param name="updateServer">是否将更新同步至服务器，默认为 true。</param>
    void UpdateLocation(MapNode visitingnode, bool updateServer = true);

    /// <summary>
    /// 更新玩家存活状态，并可选择同步至服务器。
    /// </summary>
    /// <param name="updateServer">是否将更新同步至服务器。</param>
    void UpdateLiveStatus(bool updateServer);

    //TODO:预计弃用
    /// <summary>
    /// 获取当前本地玩家实例（待弃用）。
    /// </summary>
    INetworkPlayer GetMyself();

    /// <summary>
    /// 使玩家受到指定数值的伤害。
    /// </summary>
    /// <param name="damage">伤害数值。</param>
    void Takedamage(int damage);
    //造成伤害
    /// <summary>
    /// 使玩家对目标造成指定数值的伤害。
    /// </summary>
    /// <param name="damage">伤害数值。</param>
    void DealDamage(int damage);

    /// <summary>
    /// 复活指定用户名的玩家并设置新的生命值。
    /// </summary>
    /// <param name="username">被复活玩家的用户名。</param>
    /// <param name="newhp">复活后的生命值。</param>
    void Resurrect(string username, int newhp);

    /// <summary>
    /// 将玩家传送至指定地图坐标。
    /// </summary>
    /// <param name="x">目标节点 X 坐标。</param>
    /// <param name="y">目标节点 Y 坐标。</param>
    void Teleport(int x, int y);




}
