
using System;
using System.Text.Json.Serialization;
using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 网络玩家数据模型，实现玩家的各种属性和操作。
/// </summary>
public class NetWorkPlayer
{
    //TODO:需改成SyncVar
    [JsonPropertyName("username")]
    public string username;

    [JsonPropertyName("HP")]
    public int HP;

    [JsonPropertyName("maxHP")]
    public int maxHP;

    [JsonPropertyName("block")]
    public int block;

    [JsonPropertyName("shield")]
    public int shield;

    [JsonPropertyName("coins")]
    public int coins;

    [JsonPropertyName("chara")]
    public string chara;

    [JsonPropertyName("UltimatePower")]
    public int UltimatePower;

    [JsonPropertyName("location")]
    public string location;

    [JsonPropertyName("endturn")]
    public bool endturn;

    [JsonPropertyName("mana")]
    public int[] mana;

    [JsonPropertyName("stance")]
    public string stance; //TODO:需修改

    [JsonPropertyName("exhibits")]
    public string[] exhibits;

    [JsonPropertyName("tradingStatus")]
    public bool tradingStatus;

    [JsonPropertyName("location_X")]
    public int location_X;

    [JsonPropertyName("location_Y")]
    public int location_Y;

    /// <summary>
    /// 玩家当前访问的地图节点。
    /// </summary>
    public MapNode VisitingNode { get; set; }

    /// <summary>
    /// 初始化玩家属性的构造函数。
    /// </summary>
    /// <summary>
    /// 初始化玩家属性的构造函数
    /// 设置所有玩家属性的默认值，准备游戏开始状态
    /// </summary>
    public NetWorkPlayer()
    {
        // 玩家身份信息初始化
        username = "Player"; // 默认用户名，实际游戏中应从配置获取

        // 战斗状态属性初始化
        HP = 100; // 初始生命值，标准角色的起始生命
        maxHP = 100; // 初始最大生命值，与初始HP保持一致
        block = 0; // 初始格挡值，新角色无格挡
        shield = 0; // 初始护盾值，新角色无护盾

        // 经济系统初始化
        coins = 0; // 初始金币数量，新角色无金币

        // 角色和位置信息初始化
        chara = ""; // 初始角色标识，空表示未选择角色

        UltimatePower = 0; // 初始终极能量，新角色无大招能量

        location = ""; // 初始位置描述，空表示未在特定位置

        // 回合制状态初始化
        endturn = false; // 初始回合状态，表示未结束回合

        // 资源系统初始化 - 4色法力
        mana = new int[4]; // 初始化法力数组，支持红、蓝、绿、白四色法力

        stance = ""; //TODO:需修改，初始姿态，空表示默认姿态

        // 装备系统初始化
        exhibits = new string[4]; // 初始化展品数组，最多携带4个展品

        tradingStatus = false; // 初始交易状态，表示不在交易中

        // 位置坐标初始化 - 从访问节点获取坐标
        location_X = VisitingNode.X; // 设置X坐标，与当前访问节点同步
        location_Y = VisitingNode.Y; // 设置Y坐标，与当前访问节点同步
    }

    /// <summary>
    /// 发送玩家数据到网络。
    /// </summary>
    public void SendData()
    {
        // ...existing code...
    }

    /// <summary>
    /// 判断玩家是否为房主。
    /// </summary>
    /// <returns>如果是房主返回true，否则抛出异常。</returns>
    public bool IsLobbyOwner()
    {
        throw new NotImplementedException("IsLobbyOwner method is not implemented yet."); // 未实现
    }

    /// <summary>
    /// 存档加载后执行的操作，重置回合结束标志和格挡值。
    /// </summary>
    public void PostSaveLoad()
    {
        endturn = false; // 重置回合结束标志
        block = 0; // 重置格挡
    }

    /// <summary>
    /// 判断玩家是否在同一房间。
    /// </summary>
    /// <returns>如果在同一房间返回true，否则抛出异常。</returns>
    public bool IsPlayerInSameRoom()
    {
        throw new NotImplementedException("IsPlayerInSameRoom method is not implemented yet."); // 未实现
    }

    /// <summary>
    /// 判断玩家是否在同一章节。
    /// </summary>
    /// <returns>如果在同一章节返回true，否则抛出异常。</returns>
    public bool IsPlayerOnSameAct()
    {
        throw new NotImplementedException("IsPlayerOnSameAct method is not implemented yet."); // 未实现
    }

}
