using System.Text.Json.Serialization;
using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

public class NetWorkPlayer
{
    #region Json Fields

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

        [JsonPropertyName("mood")]
    public string mood;

        [JsonPropertyName("exhibits")]
    public string[] exhibits;

        [JsonPropertyName("tradingStatus")]
    public bool tradingStatus;

        [JsonPropertyName("location_X")]
    public int location_X;

        [JsonPropertyName("location_Y")]
    public int location_Y;

    #endregion

    #region Runtime aliases

        [JsonIgnore]
    public string PlayerName
    {
        get => username;
        set => username = string.IsNullOrWhiteSpace(value) ? "Player" : value;
    }

        [JsonIgnore]
    public string CharacterId
    {
        get => chara;
        set => chara = value ?? string.Empty;
    }

        [JsonIgnore]
    public string LocationName
    {
        get => location;
        set => location = value ?? string.Empty;
    }

        [JsonIgnore]
    public int LocationX
    {
        get => location_X;
        set => location_X = value;
    }

        [JsonIgnore]
    public int LocationY
    {
        get => location_Y;
        set => location_Y = value;
    }

    #endregion

    #region Runtime-only

        public MapNode VisitingNode { get; set; }

    #endregion

        public NetWorkPlayer()
    {

        PlayerName = "Player";

        HP = 100;
        maxHP = 100;
        block = 0;
        shield = 0;

        coins = 0;

        CharacterId = "";

        UltimatePower = 0;

        LocationName = "";

        endturn = false;

        mana = new int[4];

        mood = "";

        exhibits = new string[4];

        tradingStatus = false;

        LocationX = VisitingNode?.X ?? 0;
        LocationY = VisitingNode?.Y ?? 0;
    }

}
