using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetworkPlugin.Network;

/// <summary>
/// 玩家状态快照 - 用于网络传输
/// </summary>
public class PlayerStateSnapshot
{
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("hp")]
    public int HP { get; set; }

    [JsonPropertyName("maxHP")]
    public int MaxHP { get; set; }

    [JsonPropertyName("block")]
    public int Block { get; set; }

    [JsonPropertyName("shield")]
    public int Shield { get; set; }

    [JsonPropertyName("coins")]
    public int Coins { get; set; }

    [JsonPropertyName("power")]
    public int Power { get; set; }

    [JsonPropertyName("ultimatePower")]
    public int UltimatePower { get; set; }

    [JsonPropertyName("mana")]
    public int[] Mana { get; set; } // [红, 蓝, 绿, 白]

    [JsonPropertyName("locationX")]
    public int LocationX { get; set; }

    [JsonPropertyName("locationY")]
    public int LocationY { get; set; }

    [JsonPropertyName("currentLocation")]
    public string CurrentLocation { get; set; }

    [JsonPropertyName("currentStage")]
    public int CurrentStage { get; set; }

    [JsonPropertyName("characterId")]
    public string CharacterId { get; set; }

    [JsonPropertyName("isInBattle")]
    public bool IsInBattle { get; set; }

    [JsonPropertyName("isMyTurn")]
    public bool IsMyTurn { get; set; }

    [JsonPropertyName("endTurnFlag")]
    public bool EndTurnFlag { get; set; }

    [JsonPropertyName("statusEffects")]
    public List<string> StatusEffects { get; set; }

    [JsonPropertyName("exhibits")]
    public List<string> Exhibits { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    public PlayerStateSnapshot()
    {
        StatusEffects = new List<string>();
        Exhibits = new List<string>();
        Mana = new int[4];
        Timestamp = DateTime.Now;
    }
}