using System;
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

    /// <summary>本地玩家的玩家ID</summary>
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
            // PlayerId 由服务端下发并由 NetworkIdentityTracker 维护，此处不允许客户端随意覆写。
        }
    }

    /// <summary>本地玩家的显示名称</summary>
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
                    string overrideName = config?.PlayerNameOverride?.Value;
                    if (!string.IsNullOrWhiteSpace(overrideName))
                    {
                        return overrideName;
                    }
                }
            }
            catch
            {
                // ignored
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
                // ignored
            }

            return _userName;
        }
        set => _userName = string.IsNullOrWhiteSpace(value) ? "Player" : value;
    }

    /// <summary>本地玩家的当前生命值（从本地游戏实例读取）</summary>
    public int HP
    {
        get => CurrentPlayer?.Hp ?? 0;
        set { }
    }

    /// <summary>本地玩家的最大生命值（从本地游戏实例读取）</summary>
    public int maxHP
    {
        get => CurrentPlayer?.MaxHp ?? 0;
        set { }
    }

    /// <summary>本地玩家的格挡值（从本地游戏实例读取）</summary>
    public int block
    {
        get => CurrentPlayer?.Block ?? 0;
        set { }
    }

    /// <summary>本地玩家的护盾值（从本地游戏实例读取）</summary>
    public int shield
    {
        get => CurrentPlayer?.Shield ?? 0;
        set { }
    }

    /// <summary>本地玩家的金币数量（从本地游戏实例读取）</summary>
    public int coins
    {
        get => CurrentGameRun?.Money ?? 0;
        set { }
    }

    /// <summary>本地玩家的角色标识（从本地游戏实例读取）</summary>
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

    /// <summary>本地玩家的位置名称</summary>
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

    /// <summary>本地玩家的当前章节</summary>
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
                // ignored
            }

            return _stage;
        }
        set => _stage = value;
    }

    /// <summary>是否已结束回合</summary>
    public bool endturn
    {
        get => _endTurn;
        set => _endTurn = value;
    }

    /// <summary>法力数组（红蓝绿白四色）</summary>
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

    /// <summary>本地玩家的姿势/姿态（从本地游戏实例读取）</summary>
    public string stance
    {
        get => _mood;
        set => _mood = value ?? string.Empty;
    }

    /// <summary>本地玩家的心境标识</summary>
    public string mood
    {
        get => _mood;
        set => _mood = value ?? string.Empty;
    }

    /// <summary>本地玩家的遗物列表</summary>
    public string[] exhibits
    {
        get => _exhibits;
        set => _exhibits = value ?? Array.Empty<string>();
    }

    /// <summary>交易状态标记</summary>
    public bool tradingStatus
    {
        get => _tradingStatus;
        set => _tradingStatus = value;
    }

    /// <summary>是否处于终极技能可用状态</summary>
    public bool ultimatePower
    {
        get => _ultimatePower;
        set => _ultimatePower = value;
    }

    /// <summary>地图位置X坐标</summary>
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

    /// <summary>地图位置Y坐标</summary>
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

    /// <summary>
    /// 发送玩家数据（由各 SyncPatch 负责同步，此处为空实现）
    /// </summary>
    public void SendData()
    {
        // 当前项目中，同步由各个 Patch.*SyncPatch 负责，此处保持空实现即可。
    }

    /// <summary>
    /// 判断该玩家是否为大厅房主
    /// </summary>
    /// <returns>是否为房主</returns>
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

    /// <summary>
    /// 存档/读档后的回调处理，重置本地回合状态
    /// </summary>
    public void PostSaveLoad()
    {
        endturn = false;
        block = 0;
        shield = 0;
    }

    /// <summary>判断玩家是否在同一房间</summary>
    public bool IsPlayerInSameRoom() => true;
    /// <summary>判断玩家是否处于同一章节</summary>
    public bool IsPlayerOnSameAct() => true;

    /// <summary>
    /// 处理濒死状态判定
    /// </summary>
    /// <param name="updateServer">是否需要同步到服务器</param>
    public void IsNearDeath(bool updateServer)
    {
        // 可扩展：当 HP 低于阈值时上报。
    }

    /// <summary>是否渲染角色本体</summary>
    public bool ShouldRenderCharacter() => true;
    /// <summary>是否渲染角色信息框</summary>
    public bool ShouldRenderCharacterInfoBox() => true;

    /// <summary>更新生命值显示/同步</summary>
    public void UpdateHealth(bool updateServer) { }
    /// <summary>更新格挡值显示/同步</summary>
    public void UpdateBlock(bool updateServer) { }
    /// <summary>更新最大生命值显示/同步</summary>
    public void UpdateMaxHP(bool updateServer) { }
    /// <summary>更新金币显示/同步</summary>
    public void UpdateCoins(bool updateServer) { }
    /// <summary>更新玩家信息显示/同步</summary>
    public void UpdatePlayerInfo(bool updateServer) { }
    /// <summary>更新心境显示/同步</summary>
    public void UpdateMood(bool updateServer) { }
    /// <summary>更新状态效果显示/同步</summary>
    public void UpdateStatusEffects(bool updateServer) { }
    /// <summary>更新终极技能能量显示/同步</summary>
    public void UpdateUltimatePower(bool updateServer) { }
    /// <summary>更新遗物显示/同步</summary>
    public void UpdateExhibits(bool updateServer) { }
    /// <summary>更新法力显示/同步</summary>
    public void UpdateMana(bool updateServer) { }
    /// <summary>更新结束回合标记/同步</summary>
    public void UpdateEndTurn(bool updateServer) { }

    /// <summary>
    /// 更新玩家位置信息并可选同步到服务器
    /// </summary>
    /// <param name="visitingnode">当前访问的地图节点</param>
    /// <param name="updateServer">是否同步到服务器</param>
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
            // 与 MapPanelUpdateMapNodesStatusPatch 约定的系统消息一致：UpdatePlayerLocation
            _client.SendRequest(
                "UpdatePlayerLocation",
                JsonCompat.Serialize(
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

    /// <summary>更新生存状态</summary>
    public void UpdateLiveStatus(bool updateServer) { }

    /// <summary>本地玩家受到伤害</summary>
    public void Takedamage(int damage) { }
    /// <summary>本地玩家造成伤害</summary>
    public void DealDamage(int damage) { }
    /// <summary>复活本地玩家</summary>
    public void Resurrect(string username, int newhp) { }
    /// <summary>传送本地玩家到指定坐标</summary>
    public void Teleport(int x, int y) { }
}
