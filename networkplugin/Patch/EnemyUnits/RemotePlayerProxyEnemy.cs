using LBoL.Core;
using LBoL.Core.Units;

namespace NetworkPlugin.Patch.EnemyUnits;

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
