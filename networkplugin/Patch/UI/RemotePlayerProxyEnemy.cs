using System;
using LBoL.Core;
using LBoL.Core.Units;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 将“远端玩家”映射为一个可被 TargetSelector/UnitSelector 当作 EnemyUnit 的代理目标。
/// 仅用于“被选择/被指向”，不参与 EnemyGroup，也不应被当作真实敌人结算。
/// </summary>
internal sealed class RemotePlayerProxyEnemy : EnemyUnit
{
    public string RemotePlayerId { get; }
    public string RemotePlayerName { get; private set; }

    public RemotePlayerProxyEnemy(string remotePlayerId, string remotePlayerName)
    {
        RemotePlayerId = remotePlayerId;
        RemotePlayerName = remotePlayerName;
        Initialize();
    }

    public void UpdateDisplayName(string remotePlayerName)
    {
        if (!string.IsNullOrWhiteSpace(remotePlayerName))
        {
            RemotePlayerName = remotePlayerName;
        }
    }

    public override string Name => string.IsNullOrWhiteSpace(RemotePlayerName) ? $"Remote<{RemotePlayerId}>" : RemotePlayerName;

    public override UnitName GetName()
        => UnitNameTable.GetDefaultPlayerName();

    protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
        => key == "Name" ? Name : string.Empty;
}

