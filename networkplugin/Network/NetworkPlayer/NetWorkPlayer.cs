
using System;
using System.Text.Json.Serialization;
using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 网络玩家的数据模型。
/// </summary>
/// <remarks>
/// 该类型主要用于序列化/反序列化与网络同步承载；字段名通过 <see cref="JsonPropertyNameAttribute"/> 与协议字段绑定。
/// 注意：当前实现以 public 字段为主，后续若引入 SyncVar/属性封装，需要同步更新序列化与补丁逻辑。
/// </remarks>
public class NetWorkPlayer
{
    #region Json Fields

    // TODO: 后续可改为 SyncVar/属性封装（需同时调整序列化与同步逻辑）。

    /// <summary>
    /// 玩家名称/标识。
    /// </summary>
    [JsonPropertyName("username")]
    public string username;

    /// <summary>
    /// 当前生命值。
    /// </summary>
    [JsonPropertyName("HP")]
    public int HP;

    /// <summary>
    /// 最大生命值。
    /// </summary>
    [JsonPropertyName("maxHP")]
    public int maxHP;

    /// <summary>
    /// 格挡值。
    /// </summary>
    [JsonPropertyName("block")]
    public int block;

    /// <summary>
    /// 护盾值。
    /// </summary>
    [JsonPropertyName("shield")]
    public int shield;

    /// <summary>
    /// 金币数量。
    /// </summary>
    [JsonPropertyName("coins")]
    public int coins;

    /// <summary>
    /// 角色标识（例如角色/模型名）。
    /// </summary>
    [JsonPropertyName("chara")]
    public string chara;

    /// <summary>
    /// 终极能量/充能值。
    /// </summary>
    [JsonPropertyName("UltimatePower")]
    public int UltimatePower;

    /// <summary>
    /// 位置名称（可由地图节点站点类型派生）。
    /// </summary>
    [JsonPropertyName("location")]
    public string location;

    /// <summary>
    /// 是否已结束回合。
    /// </summary>
    [JsonPropertyName("endturn")]
    public bool endturn;

    /// <summary>
    /// 法力数组（通常为红、蓝、绿、白四种）。
    /// </summary>
    [JsonPropertyName("mana")]
    public int[] mana;

    /// <summary>
    /// 战斗姿态标识。
    /// </summary>
    [JsonPropertyName("mood")]
    public string mood; // TODO: 协议/枚举化需要统一。

    /// <summary>
    /// 展品列表。
    /// </summary>
    [JsonPropertyName("exhibits")]
    public string[] exhibits;

    /// <summary>
    /// 交易状态。
    /// </summary>
    [JsonPropertyName("tradingStatus")]
    public bool tradingStatus;

    /// <summary>
    /// 位置 X 坐标。
    /// </summary>
    [JsonPropertyName("location_X")]
    public int location_X;

    /// <summary>
    /// 位置 Y 坐标。
    /// </summary>
    [JsonPropertyName("location_Y")]
    public int location_Y;

    #endregion

    #region Runtime-only

    /// <summary>
    /// 玩家当前访问的地图节点（运行时引用）。
    /// </summary>
    /// <remarks>
    /// 该属性通常不参与 JSON 协议字段映射；更多用于本地逻辑关联。
    /// </remarks>
    public MapNode VisitingNode { get; set; }

    #endregion

    /// <summary>
    /// 初始化 <see cref="NetWorkPlayer"/>。
    /// </summary>
    /// <remarks>
    /// 仅设置默认值，具体数值应在进入局内/同步时更新。
    /// 注意：构造函数末尾访问 <see cref="VisitingNode"/> 坐标前，需要确保其已被赋值。
    /// </remarks>
    public NetWorkPlayer()
    {
        // 身份信息
        username = "Player"; // 默认用户名（实际应由外部配置/同步赋值）

        // 战斗状态
        HP = 100; // 默认生命值
        maxHP = 100; // 默认最大生命值
        block = 0; // 默认格挡
        shield = 0; // 默认护盾

        // 经济
        coins = 0; // 默认金币

        // 角色/位置
        chara = ""; // 默认角色标识

        UltimatePower = 0; // 默认终极能量

        location = ""; // 默认位置名称

        // 回合
        endturn = false; // 默认未结束回合

        // 资源：四色法力
        mana = new int[4]; // 默认法力数组

        mood = ""; // 默认姿态标识

        // 装备
        exhibits = new string[4]; // 默认展品数组

        tradingStatus = false; // 默认不在交易中

        // 坐标：从访问节点同步（需确保 VisitingNode 非空）
        location_X = VisitingNode?.X ?? 0; // 与访问节点同步 X（无节点时回退0）
        location_Y = VisitingNode?.Y ?? 0; // 与访问节点同步 Y（无节点时回退0）
    }

    /// <summary>
    /// 发送玩家数据到网络。
    /// </summary>
    /// <remarks>
    /// 当前方法体为占位，具体实现应由网络层/同步补丁负责。
    /// </remarks>
    public void SendData()
    {
        // ...existing code...
    }

    /// <summary>
    /// 判断玩家是否为房主。
    /// </summary>
    /// <returns>当前实现未提供可用结果。</returns>
    /// <exception cref="NotImplementedException">当前版本尚未实现。</exception>
    public bool IsLobbyOwner()
    {
        throw new NotImplementedException("IsLobbyOwner method is not implemented yet."); // 未实现
    }

    /// <summary>
    /// 存档加载后的状态重置。
    /// </summary>
    public void PostSaveLoad()
    {
        endturn = false; // 回合结束标记复位
        block = 0; // 格挡复位
    }

    /// <summary>
    /// 判断玩家是否在同一房间。
    /// </summary>
    /// <returns>当前实现未提供可用结果。</returns>
    /// <exception cref="NotImplementedException">当前版本尚未实现。</exception>
    public bool IsPlayerInSameRoom()
    {
        throw new NotImplementedException("IsPlayerInSameRoom method is not implemented yet."); // 未实现
    }

    /// <summary>
    /// 判断玩家是否在同一章节（Act）。
    /// </summary>
    /// <returns>当前实现未提供可用结果。</returns>
    /// <exception cref="NotImplementedException">当前版本尚未实现。</exception>
    public bool IsPlayerOnSameAct()
    {
        throw new NotImplementedException("IsPlayerOnSameAct method is not implemented yet."); // 未实现
    }

    // 说明：如后续补齐 INetworkPlayer 能力，可在此区域继续扩展对应方法。

}
