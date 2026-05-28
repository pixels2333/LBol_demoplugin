using LBoL.Core;
using LBoL.Core.Units;

namespace NetworkPlugin.Patch.EnemyUnits;

/// <summary>
/// 将“远端玩家”映射为一个可被 TargetSelector/UnitSelector 当作 EnemyUnit 的代理目标。
/// 仅用于“被选择/被指向”，不参与 EnemyGroup，也不应被当作真实敌人结算。
/// </summary>
internal sealed class RemotePlayerProxyEnemy : EnemyUnit
{
    public string RemotePlayerId { get; }
    public string RemotePlayerName { get; private set; }

    /// <summary>
    /// 初始化远端玩家代理敌方单位
    /// </summary>
    /// <param name="remotePlayerId">远端玩家 ID</param>
    /// <param name="remotePlayerName">远端玩家名称</param>
    public RemotePlayerProxyEnemy(string remotePlayerId, string remotePlayerName)
    {
        RemotePlayerId = remotePlayerId;
        RemotePlayerName = remotePlayerName;
        Initialize();
    }

    /// <summary>
    /// 更新显示的远端玩家名称
    /// </summary>
    /// <param name="remotePlayerName">新的玩家名称</param>
    public void UpdateDisplayName(string remotePlayerName)
    {
        if (!string.IsNullOrWhiteSpace(remotePlayerName))
        {
            RemotePlayerName = remotePlayerName;
        }
    }

    public override string Name => string.IsNullOrWhiteSpace(RemotePlayerName) ? $"Remote<{RemotePlayerId}>" : RemotePlayerName;

    /// <summary>
    /// 获取默认的玩家名称（模拟 UnitName）
    /// </summary>
    public override UnitName GetName()
        => UnitNameTable.GetDefaultPlayerName();

    /// <summary>
    /// 本地化属性：仅 "Name" 返回当前显示名称
    /// </summary>
    protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
        => key == "Name" ? Name : string.Empty;
}
