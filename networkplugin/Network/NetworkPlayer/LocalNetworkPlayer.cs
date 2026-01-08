using System;
using System.Text.Json;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 本地玩家的 <see cref="INetworkPlayer"/> 轻量实现：
/// - 主要用于给现有补丁提供稳定的 player.userName/HP/block/mana 等读数。
/// - 不强行承担“真正的联机权威同步”，同步职责仍以各 SyncPatch 为主。
/// </summary>
public sealed class LocalNetworkPlayer : INetworkPlayer
{
    private readonly INetworkClient _client;

    private string _userName = "Player";
    private string _chara = string.Empty;
    private string _location = string.Empty;
    private bool _endTurn;
    private string _stance = string.Empty;
    private string[] _exhibits = Array.Empty<string>();
    private bool _tradingStatus;
    private bool _ultimatePower;
    private int[] _mana = new int[4];
    private int _locationX;
    private int _locationY;

    public LocalNetworkPlayer(INetworkClient client)
    {
        _client = client;
    }

    private static PlayerUnit CurrentPlayer => GameStateUtils.GetCurrentPlayer();
    private static GameRunController CurrentGameRun => GameStateUtils.GetCurrentGameRun();

    public string userName
    {
        get
        {
            try
            {
                // 优先使用服务器侧分配的 PlayerId（便于排查），其次使用游戏内角色名。
                string id = NetworkIdentityTracker.GetSelfPlayerId();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    return id;
                }

                string name = CurrentPlayer?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }
            catch
            {
                // ignored
            }

            return _userName;
        }
        set => _userName = string.IsNullOrWhiteSpace(value) ? "Player" : value;
    }

    public int HP
    {
        get => CurrentPlayer?.Hp ?? 0;
        set { }
    }

    public int maxHP
    {
        get => CurrentPlayer?.MaxHp ?? 0;
        set { }
    }

    public int block
    {
        get => CurrentPlayer?.Block ?? 0;
        set { }
    }

    public int shield
    {
        get => CurrentPlayer?.Shield ?? 0;
        set { }
    }

    public int coins
    {
        get => CurrentGameRun?.Money ?? 0;
        set { }
    }

    public string chara
    {
        get
        {
            try
            {
                string model = CurrentPlayer?.ModelName;
                if (!string.IsNullOrWhiteSpace(model))
                {
                    return model;
                }
            }
            catch
            {
                // ignored
            }

            return _chara;
        }
        set => _chara = value ?? string.Empty;
    }

    public string location
    {
        get
        {
            try
            {
                MapNode node = CurrentGameRun?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    return node.StationType.ToString();
                }
            }
            catch
            {
                // ignored
            }

            return _location;
        }
        set => _location = value ?? string.Empty;
    }

    public bool endturn
    {
        get => _endTurn;
        set => _endTurn = value;
    }

    public int[] mana
    {
        get
        {
            try
            {
                ManaGroup mg = (ManaGroup)(CurrentPlayer?.Battle?.BattleMana);
                if (mg != null)
                {
                    _mana[0] = mg.Red;
                    _mana[1] = mg.Blue;
                    _mana[2] = mg.Green;
                    _mana[3] = mg.White;
                }
            }
            catch
            {
                // ignored
            }

            return _mana;
        }
        set => _mana = value ?? new int[4];
    }

    public string stance
    {
        get => _stance;
        set => _stance = value ?? string.Empty;
    }

    public string[] exhibits
    {
        get => _exhibits;
        set => _exhibits = value ?? Array.Empty<string>();
    }

    public bool tradingStatus
    {
        get => _tradingStatus;
        set => _tradingStatus = value;
    }

    public bool ultimatePower
    {
        get => _ultimatePower;
        set => _ultimatePower = value;
    }

    public int location_X
    {
        get
        {
            try
            {
                MapNode node = CurrentGameRun?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    return node.X;
                }
            }
            catch
            {
                // ignored
            }

            return _locationX;
        }
        set => _locationX = value;
    }

    public int location_Y
    {
        get
        {
            try
            {
                MapNode node = CurrentGameRun?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    return node.Y;
                }
            }
            catch
            {
                // ignored
            }

            return _locationY;
        }
        set => _locationY = value;
    }

    public void SendData()
    {
        // 当前项目中，同步由各个 Patch.*SyncPatch 负责，此处保持空实现即可。
    }

    public bool IsLobbyOwner()
    {
        try
        {
            return NetworkIdentityTracker.GetSelfIsHost();
        }
        catch
        {
            return false;
        }
    }

    public void PostSaveLoad()
    {
        endturn = false;
        block = 0;
        shield = 0;
    }

    public bool IsPlayerInSameRoom() => true;
    public bool IsPlayerOnSameAct() => true;

    public void IsNearDeath(bool updateServer)
    {
        // 可扩展：当 HP 低于阈值时上报。
    }

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

    public void UpdateLocation(MapNode visitingnode, bool updateServer = true)
    {
        if (visitingnode != null)
        {
            location_X = visitingnode.X;
            location_Y = visitingnode.Y;
            location = visitingnode.StationType.ToString();
        }

        if (!updateServer || _client?.IsConnected != true || visitingnode == null)
        {
            return;
        }

        try
        {
            // 与 MapPanelUpdateMapNodesStatusPatch 约定的系统消息一致：UpdatePlayerLocation
            _client.SendRequest(
                "UpdatePlayerLocation",
                JsonSerializer.Serialize(
                    new
                    {
                        LocationX = visitingnode.X,
                        LocationY = visitingnode.Y,
                        LocationName = visitingnode.StationType.ToString(),
                        LocationType = visitingnode.GetType().Name,
                        Stage = visitingnode.Act
                    }
                )
            );
        }
        catch
        {
            // ignored
        }
    }

    public void UpdateLiveStatus(bool updateServer) { }

    public INetworkPlayer GetMyself() => this;

    public void Takedamage(int damage) { }
    public void DealDamage(int damage) { }
    public void Resurrect(string username, int newhp) { }
    public void Teleport(int x, int y) { }
}

