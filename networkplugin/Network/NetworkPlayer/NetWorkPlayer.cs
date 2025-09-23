using System;
using System.Collections.Generic;


namespace NetworkPlugin.Network.NetworkPlayer;

public class NetWorkPlayer
{

    //TODO:需改成SyncVar
    private string username = "Player";
    private int HP = 100;
    private int maxHP = 100;
    private int block = 0;
    private int shield = 0;

    private int coins = 0;

    private string chara = "";

    private int UltimatePower = 0;

    private string location = "";

    private bool endturn = false;

    private int[] mana = new int[4];

    private string stance = "";//TODO:需修改

    private string[] relics = new string[4];

    private bool tradingStatus = false;

    

}
