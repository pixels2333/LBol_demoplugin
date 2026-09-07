using System;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.NetworkPlayer;

public sealed class LocalNetworkPlayer : INetworkPlayer
{
    private readonly INetworkClient _client;

    private string _userName = "Player";
    private string _chara = string.Empty;
    private string _location = string.Empty;
    private bool _endTurn;
    private string _mood = string.Empty;
    private string[] _exhibits = Array.Empty<string>();
    private bool _tradingStatus;
    private bool _ultimatePower;
    private int[] _mana = new int[4];
    private int _locationX;
    private int _locationY;
    private int _stage = -1;

    public LocalNetworkPlayer(INetworkClient client)
    {
        _client = client;
    }

    private static PlayerUnit CurrentPlayer => GameStateUtils.GetCurrentPlayer();
    private static GameRunController CurrentGameRun => GameStateUtils.GetCurrentGameRun();

        public string playerId
    {
        get
        {
            try
            {
                NetworkIdentityTracker.EnsureSubscribed(_client);
                return NetworkIdentityTracker.GetSelfPlayerId();
            }
            catch
            {
                return null;
            }
        }
        set
        {

        }
    }

        public string userName
    {
        get
        {
            try
            {
                var provider = NetworkPlugin.Network.Services.ModService.ServiceProvider;
                if (provider != null)
                {
                    var config = provider.GetService(typeof(NetworkPlugin.Configuration.ConfigManager)) as NetworkPlugin.Configuration.ConfigManager;
                    string overrideName = null;
                    if (NetworkPlugin.Patch.UI.MainMenuMultiplayerEntryPatch.IsLocalServerRunning)
                    {
                        overrideName = config?.HostPlayerNameOverride?.Value;
                    }
                    if (string.IsNullOrWhiteSpace(overrideName))
                    {
                        overrideName = config?.PlayerNameOverride?.Value;
                    }
                    if (!string.IsNullOrWhiteSpace(overrideName))
                    {
                        return overrideName;
                    }
                }
            }
            catch
            {

            }

            try
            {
                string name = CurrentPlayer?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }
            catch
            {

            }

            return _userName;
        }
        set => _userName = string.IsNullOrWhiteSpace(value) ? "Player" : value;
    }

        public int Power
    {
        get => CurrentPlayer?.Power ?? 0;
        set { }
    }

        public int PowerPerLevel
    {
        get => CurrentPlayer?.Us != null ? CurrentPlayer.Us.PowerPerLevel : 100;
        set { }
    }

        public int MaxPowerLevel
    {
        get => CurrentPlayer?.Us != null ? CurrentPlayer.Us.MaxPowerLevel : 3;
        set { }
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

            }

            return _location;
        }
        set => _location = value ?? string.Empty;
    }

        public int stage
    {
        get
        {
            try
            {
                MapNode node = CurrentGameRun?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    return node.Act;
                }
            }
            catch
            {

            }

            return _stage;
        }
        set => _stage = value;
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

            }

            return _mana;
        }
        set => _mana = value ?? new int[4];
    }

        public string stance
    {
        get => _mood;
        set => _mood = value ?? string.Empty;
    }

        public string mood
    {
        get => _mood;
        set => _mood = value ?? string.Empty;
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

            }

            return _locationY;
        }
        set => _locationY = value;
    }

        public void SendData()
    {

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

    }

        public bool ShouldRenderCharacter() => true;
        public bool ShouldRenderCharacterInfoBox() => true;

        public void UpdateHealth(bool updateServer) { }
        public void UpdateBlock(bool updateServer) { }
        public void UpdateMaxHP(bool updateServer) { }
        public void UpdateCoins(bool updateServer) { }
        public void UpdatePlayerInfo(bool updateServer) { }
        public void UpdateMood(bool updateServer) { }
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
            stage = visitingnode.Act;
        }

        if (!updateServer || _client?.IsConnected != true || visitingnode == null)
        {
            return;
        }

        try
        {
            string characterId = null;
            try
            {
                characterId = CurrentPlayer?.ModelName;
            }
            catch
            {

            }

            _client.SendRequest(
                "UpdatePlayerLocation",
                JsonCompat.Serialize(
                    new
                    {
                        LocationX = visitingnode.X,
                        LocationY = visitingnode.Y,
                        LocationName = visitingnode.StationType.ToString(),
                        LocationType = visitingnode.GetType().Name,
                        Stage = visitingnode.Act,
                        CharacterId = characterId
                    }
                )
            );
        }
        catch
        {

        }
    }

        public void UpdateLiveStatus(bool updateServer) { }

        public void Takedamage(int damage) { }
        public void DealDamage(int damage) { }
        public void Resurrect(string username, int newhp) { }
        public void Teleport(int x, int y) { }
}
