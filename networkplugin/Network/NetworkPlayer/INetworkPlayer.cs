
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

    void IsNearDeath(bool updateServer);

    bool ShouldRenderCharacter();

    bool ShouldRenderCharacterInfoBox();

    void UpdateHealth(bool updateServer);

    void UpdateBlock(bool updateServer);

    void UpdateMaxHP(bool updateServer);

    void UpdateCoins(bool updateServer);

    void UpdatePlayerInfo(bool updateServer);

    //TODO:stance名称可能要改
    void UpdateStance(bool updateServer);

    // void ClearPowers(bool updateServer);

    // void UpdatePowers(bool updateServer);

    // void UpdateTempPowers(bool updateServer);
    void UpdateStatusEffects(bool updateServer);

    void UpdateUltimatePower(bool updateServer);

    void UpdateExhibits(bool updateServer);

    void UpdateMana(bool updateServer);

    void UpdateEndTurn(bool updateServer);

    void UpdateLocation(MapNode visitingnode,bool updateServer=true);

    void UpdateLiveStatus(bool updateServer);

    //TODO:预计弃用
    INetworkPlayer GetMyself();

    void Takedamage(int damage);
    //造成伤害
    void DealDamage(int damage);

    void Resurrect(string username, int newhp);

    void Teleport(int x, int y);




}

