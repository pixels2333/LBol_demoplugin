using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetworkPlugin.Network;

/// <summary>
/// 玩家实体类 - 使用SyncVar实现自动同步的核心玩家数据
/// 所有需要网络同步的玩家状态都应该在这里定义
/// </summary>
public class PlayerEntity
{
    // 玩家标识
    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("isHost")]
    public bool IsHost { get; set; }

    // 基础状态属性 - 使用SyncVar自动同步
    public SyncVar<int> HP { get; private set; }
    public SyncVar<int> MaxHP { get; private set; }
    public SyncVar<int> Block { get; private set; }
    public SyncVar<int> Shield { get; private set; }
    public SyncVar<int> Coins { get; private set; }

    // 资源
    public SyncVar<int> Power { get; private set; }
    public SyncVar<int> UltimatePower { get; private set; }
    public SyncVar<int[]> Mana { get; private set; } // 4色法力: 0=红,1=蓝,2=绿,3=白

    // 位置与进度
    public SyncVar<int> LocationX { get; private set; }
    public SyncVar<int> LocationY { get; private set; }
    public SyncVar<string> CurrentLocation { get; private set; }
    public SyncVar<int> CurrentStage { get; private set; }
    public SyncVar<string> CharacterId { get; private set; }

    // 回合状态
    public SyncVar<bool> IsInBattle { get; private set; }
    public SyncVar<bool> IsMyTurn { get; private set; }
    public SyncVar<bool> EndTurnFlag { get; private set; }

    // 状态效果 (存储状态效果的ID列表)
    public SyncVar<List<string>> ActiveStatusEffects { get; private set; }

    // 宝物 (存储宝物的ID列表)
    public SyncVar<List<string>> Exhibits { get; private set; }

    // 在线状态
    public SyncVar<bool> IsConnected { get; private set; }
    public DateTime LastUpdate { get; private set; }

    public PlayerEntity(string playerId, string username, bool isHost = false)
    {
        PlayerId = playerId;
        Username = username;
        IsHost = isHost;

        // 初始化SyncVar属性
        HP = new SyncVar<int>(nameof(HP), 100);
        MaxHP = new SyncVar<int>(nameof(MaxHP), 100);
        Block = new SyncVar<int>(nameof(Block), 0);
        Shield = new SyncVar<int>(nameof(Shield), 0);
        Coins = new SyncVar<int>(nameof(Coins), 0);

        Power = new SyncVar<int>(nameof(Power), 0);
        UltimatePower = new SyncVar<int>(nameof(UltimatePower), 0);
        Mana = new SyncVar<int[]>(nameof(Mana), new int[4]); // 4色法力

        LocationX = new SyncVar<int>(nameof(LocationX), 0);
        LocationY = new SyncVar<int>(nameof(LocationY), 0);
        CurrentLocation = new SyncVar<string>(nameof(CurrentLocation), "");
        CurrentStage = new SyncVar<int>(nameof(CurrentStage), 1);
        CharacterId = new SyncVar<string>(nameof(CharacterId), "");

        IsInBattle = new SyncVar<bool>(nameof(IsInBattle), false);
        IsMyTurn = new SyncVar<bool>(nameof(IsMyTurn), false);
        EndTurnFlag = new SyncVar<bool>(nameof(EndTurnFlag), false);

        ActiveStatusEffects = new SyncVar<List<string>>(nameof(ActiveStatusEffects), new List<string>());
        Exhibits = new SyncVar<List<string>>(nameof(Exhibits), new List<string>());

        IsConnected = new SyncVar<bool>(nameof(IsConnected), true);
    }

    /// <summary>
    /// 更新最后更新时间
    /// </summary>
    public void UpdateTimestamp()
    {
        LastUpdate = DateTime.Now;
    }

    /// <summary>
    /// 重置战斗状态(战斗结束后调用)
    /// </summary>
    public void ResetBattleState()
    {
        Block.Value = 0;
        Shield.Value = 0;
        ActiveStatusEffects.Value.Clear();
        IsInBattle.Value = false;
        IsMyTurn.Value = false;
        EndTurnFlag.Value = false;
    }

    /// <summary>
    /// 快照当前状态(用于网络传输)
    /// </summary>
    public PlayerStateSnapshot CreateSnapshot()
    {
        return new PlayerStateSnapshot
        {
            PlayerId = PlayerId,
            Username = Username,
            HP = HP.Value,
            MaxHP = MaxHP.Value,
            Block = Block.Value,
            Shield = Shield.Value,
            Coins = Coins.Value,
            Power = Power.Value,
            UltimatePower = UltimatePower.Value,
            Mana = (int[])Mana.Value.Clone(),
            LocationX = LocationX.Value,
            LocationY = LocationY.Value,
            CurrentLocation = CurrentLocation.Value,
            CurrentStage = CurrentStage.Value,
            CharacterId = CharacterId.Value,
            IsInBattle = IsInBattle.Value,
            IsMyTurn = IsMyTurn.Value,
            EndTurnFlag = EndTurnFlag.Value,
            StatusEffects = new List<string>(ActiveStatusEffects.Value),
            Exhibits = new List<string>(Exhibits.Value),
            Timestamp = LastUpdate
        };
    }

    /// <summary>
    /// 从快照恢复状态
    /// </summary>
    public void ApplySnapshot(PlayerStateSnapshot snapshot)
    {
        HP.Value = snapshot.HP;
        MaxHP.Value = snapshot.MaxHP;
        Block.Value = snapshot.Block;
        Shield.Value = snapshot.Shield;
        Coins.Value = snapshot.Coins;
        Power.Value = snapshot.Power;
        UltimatePower.Value = snapshot.UltimatePower;
        Mana.Value = (int[])snapshot.Mana.Clone();
        LocationX.Value = snapshot.LocationX;
        LocationY.Value = snapshot.LocationY;
        CurrentLocation.Value = snapshot.CurrentLocation;
        CurrentStage.Value = snapshot.CurrentStage;
        CharacterId.Value = snapshot.CharacterId;
        IsInBattle.Value = snapshot.IsInBattle;
        IsMyTurn.Value = snapshot.IsMyTurn;
        EndTurnFlag.Value = snapshot.EndTurnFlag;
        ActiveStatusEffects.Value = new List<string>(snapshot.StatusEffects);
        Exhibits.Value = new List<string>(snapshot.Exhibits);
        LastUpdate = snapshot.Timestamp;
    }
}
