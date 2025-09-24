using System;
using System.Collections.Generic;


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


    public bool IsPlayerInSameRoom() {
      if (CardCrawlGame.dungeon != null && AbstractDungeon.currMapNode != null) {
         if (this.location != null) {
            return this.location.equals(P2PManager.GetSelf().location);
         } else {
            // 如果位置为空，但在 Neow 房间，则视为在同一房间
            return this.startStatus == P2PPlayer.StartStatus.EMBARKED && AbstractDungeon.getCurrRoom() instanceof NeowRoom;
         }
      } else {
         return false;
      }
   }




}
