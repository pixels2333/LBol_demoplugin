using System;
using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 远程玩家的轻量 <see cref="INetworkPlayer"/> 实现：
/// - 仅用于在本地维护“玩家列表/数量”等逻辑，避免大量 Patch 因缺少玩家对象而报错。
/// - 远程玩家的真实战斗/状态同步由各个 *SyncPatch 负责，本类方法保持空实现或保守默认值。
/// </summary>
public sealed class RemoteNetworkPlayer : INetworkPlayer
{
    public RemoteNetworkPlayer(string userName)
    {
        this.userName = string.IsNullOrWhiteSpace(userName) ? "Player" : userName;
        mana = new int[4];
        exhibits = Array.Empty<string>();
    }

    public string userName { get; set; }
    public int HP { get; set; }
    public int maxHP { get; set; }
    public int block { get; set; }
    public int shield { get; set; }
    public int coins { get; set; }
    public string chara { get; set; } = string.Empty;
    public string location { get; set; } = string.Empty;
    public bool endturn { get; set; }
    public int[] mana { get; set; }
    public string stance { get; set; } = string.Empty;
    public string[] exhibits { get; set; }
    public bool tradingStatus { get; set; }
    public bool ultimatePower { get; set; }
    public int location_X { get; set; } = -1;
    public int location_Y { get; set; } = -1;

    public void SendData() { }
    public bool IsLobbyOwner() => false;
    public void PostSaveLoad() { }
    public bool IsPlayerInSameRoom() => true;
    public bool IsPlayerOnSameAct() => true;

    public void IsNearDeath(bool updateServer) { }
    public bool ShouldRenderCharacter() => true;
    public bool ShouldRenderCharacterInfoBox() => true;
    public void UpdateHealth(bool updateServer) { }
    public void UpdateBlock(bool updateServer) { }
    public void UpdateMaxHP(bool updateServer) { }
    public void UpdateCoins(bool updateServer) { }
    public void UpdatePlayerInfo(bool updateServer) { }
    public void UpdateStance(bool updateServer) { }
    public void UpdateStatusEffects(bool updateServer) { }
    public void UpdateUltimatePower(bool updateServer) { }
    public void UpdateExhibits(bool updateServer) { }
    public void UpdateMana(bool updateServer) { }
    public void UpdateEndTurn(bool updateServer) { }
    public void UpdateLocation(MapNode visitingnode, bool updateServer = true) { }
    public void UpdateLiveStatus(bool updateServer) { }

    public INetworkPlayer GetMyself() => this;
    public void Takedamage(int damage) { }
    public void DealDamage(int damage) { }
    public void Resurrect(string username, int newhp) { }
    public void Teleport(int x, int y) { }
}

