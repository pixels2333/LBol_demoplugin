
using System;
using System.Collections.Generic;
using LBoL.Core;
using System.Text.Json.Serialization;

namespace NetworkPlugin.Network.NetworkPlayer;

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

   
   public MapNode VisitingNode { get; set; }

   public NetWorkPlayer()
   {
      username = "Player";

      HP = 100;

      maxHP = 100;

      block = 0;

      shield = 0;

      coins = 0;

      chara = "";

      UltimatePower = 0;

      location = "";

      endturn = false;

      mana = new int[4];

      stance = ""; //TODO:需修改

      exhibits = new string[4];

      tradingStatus = false;

      location_X = VisitingNode.X;

      location_Y = VisitingNode.Y;


   }
   public void SendData()
   {


   }

   public bool IsLobbyOwner()
   {
      throw new NotImplementedException("IsLobbyOwner method is not implemented yet.");
   }

   public void PostSaveLoad()
   {
      endturn = false; // 重置回合结束标志
      block = 0; // 重置格挡
   }


   public bool IsPlayerInSameRoom()
   {
      throw new NotImplementedException("IsPlayerInSameRoom method is not implemented yet.");


   }

}
