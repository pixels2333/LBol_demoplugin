using System;
using System.Collections.Generic;
using LBoL.Core;


namespace NetworkPlugin.Network.NetworkPlayer;

public class NetWorkPlayer
{

   //TODO:需改成SyncVar
   private string username;

   private int HP;

   private int maxHP;

   private int block;

   private int shield;


   private int coins;

   private string chara;

   private int UltimatePower;

   private string location;

   private bool endturn;

   private int[] mana;

   private string stance; //TODO:需修改

   private string[] exhibits;

   private bool tradingStatus;

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
