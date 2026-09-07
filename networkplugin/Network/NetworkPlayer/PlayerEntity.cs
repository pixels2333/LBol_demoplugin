using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Network.Sync;

namespace NetworkPlugin.Network.NetworkPlayer;

public class PlayerEntity
{

        [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

        [JsonPropertyName("username")]
    public string Username { get; set; }

        [JsonPropertyName("isHost")]
    public bool IsHost { get; set; }

        public SyncVar<int> HP { get; private set; }
        public SyncVar<int> MaxHP { get; private set; }
        public SyncVar<int> Block { get; private set; }
        public SyncVar<int> Shield { get; private set; }
        public SyncVar<int> Coins { get; private set; }

        public SyncVar<int> Power { get; private set; }
        public SyncVar<int> UltimatePower { get; private set; }
        public SyncVar<int[]> Mana { get; private set; }

        public SyncVar<int> LocationX { get; private set; }
        public SyncVar<int> LocationY { get; private set; }
        public SyncVar<string> CurrentLocation { get; private set; }
        public SyncVar<int> CurrentStage { get; private set; }
        public SyncVar<string> CharacterId { get; private set; }

        public SyncVar<bool> IsInBattle { get; private set; }
        public SyncVar<bool> IsMyTurn { get; private set; }
        public SyncVar<bool> EndTurnFlag { get; private set; }

        public SyncVar<List<string>> ActiveStatusEffects { get; private set; }

        public SyncVar<List<string>> Exhibits { get; private set; }

        public SyncVar<bool> IsConnected { get; private set; }
        public DateTime LastUpdate { get; private set; }

        public PlayerEntity(string playerId, string username, bool isHost = false)
    {

        PlayerId = playerId;
        Username = username;
        IsHost = isHost;

        HP = new SyncVar<int>(nameof(HP), 100);
        MaxHP = new SyncVar<int>(nameof(MaxHP), 100);
        Block = new SyncVar<int>(nameof(Block), 0);
        Shield = new SyncVar<int>(nameof(Shield), 0);
        Coins = new SyncVar<int>(nameof(Coins), 0);

        Power = new SyncVar<int>(nameof(Power), 0);
        UltimatePower = new SyncVar<int>(nameof(UltimatePower), 0);
        Mana = new SyncVar<int[]>(nameof(Mana), new int[4]);

        LocationX = new SyncVar<int>(nameof(LocationX), 0);
        LocationY = new SyncVar<int>(nameof(LocationY), 0);
        CurrentLocation = new SyncVar<string>(nameof(CurrentLocation), "");
        CurrentStage = new SyncVar<int>(nameof(CurrentStage), 1);
        CharacterId = new SyncVar<string>(nameof(CharacterId), "");

        IsInBattle = new SyncVar<bool>(nameof(IsInBattle), false);
        IsMyTurn = new SyncVar<bool>(nameof(IsMyTurn), false);
        EndTurnFlag = new SyncVar<bool>(nameof(EndTurnFlag), false);

        ActiveStatusEffects = new SyncVar<List<string>>(nameof(ActiveStatusEffects), []);
        Exhibits = new SyncVar<List<string>>(nameof(Exhibits), []);

        IsConnected = new SyncVar<bool>(nameof(IsConnected), true);
    }

        public void UpdateTimestamp()
    {
        LastUpdate = DateTime.Now;
    }

        public void ResetBattleState()
    {
        Block.Value = 0;
        Shield.Value = 0;
        ActiveStatusEffects.Value.Clear();
        IsInBattle.Value = false;
        IsMyTurn.Value = false;
        EndTurnFlag.Value = false;
    }

        public PlayerStateSnapshot CreateSnapshot()
    {
        return new PlayerStateSnapshot()
        {
            PlayerId = PlayerId,
            Timestamp = DateTime.Now,
            Health = HP.Value,
            MaxHealth = MaxHP.Value,
            Block = Block.Value,
            Shield = Shield.Value,
            ManaGroup = Mana.Value,
            Gold = Coins.Value,
            Cards = [],
            Exhibits = [],
            ToolCards = [],
            StatusEffects = [],
            GameLocation = new LocationSnapshot() { X = LocationX.Value, Y = LocationY.Value },
            IsInBattle = IsInBattle.Value,
        };
    }

        public void ApplySnapshot(PlayerStateSnapshot snapshot)
    {
        HP.Value = snapshot.Health;
        MaxHP.Value = snapshot.MaxHealth;
        Block.Value = snapshot.Block;
        Shield.Value = snapshot.Shield;
        Coins.Value = snapshot.Gold;
        Mana.Value = (int[])snapshot.ManaGroup.Clone();
        LocationX.Value = snapshot.GameLocation.X;
        LocationY.Value = snapshot.GameLocation.Y;
        IsInBattle.Value = snapshot.IsInBattle;
        ActiveStatusEffects.Value = snapshot.StatusEffects.Select(e => e.ToString()).ToList();
        Exhibits.Value = snapshot.Exhibits.Select(e => e.ToString()).ToList();
        LastUpdate = snapshot.Timestamp;
    }
    }
