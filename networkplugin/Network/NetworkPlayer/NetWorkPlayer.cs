
using System;
using System.Collections.Generic;
using LBoL.Core;
using System.Text.Json.Serialization;

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
   public NetWorkPlayer()
   {
      username = "Player"; // 默认用户名

      HP = 100; // 初始生命值

      maxHP = 100; // 初始最大生命值

      block = 0; // 初始格挡值

      shield = 0; // 初始护盾值

      coins = 0; // 初始金币数量

      chara = ""; // 初始角色标识

      UltimatePower = 0; // 初始终极能量

      location = ""; // 初始位置描述

      endturn = false; // 初始回合状态

      mana = new int[4]; // 初始化法力数组

      stance = ""; //TODO:需修改，初始姿态

      exhibits = new string[4]; // 初始化展品数组

      tradingStatus = false; // 初始交易状态

      location_X = VisitingNode.X; // 设置X坐标

      location_Y = VisitingNode.Y; // 设置Y坐标
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
