using LBoL.Core;

namespace NetworkPlugin.Network.NetworkPlayer;

/// <summary>
/// 网络玩家接口（复合接口）
/// 聚合了 <see cref="IPlayerIdentity"/>、<see cref="IPlayerBattleState"/>、
/// <see cref="IPlayerResources"/>、<see cref="IPlayerNetworkSync"/> 四个子接口。
///
/// 保持此接口不变以保证向后兼容；新代码可选用更精细的子接口。
/// </summary>
/// <remarks>
/// 说明：为与联机协议/序列化字段保持一致，本接口成员命名可能不遵循常规 C# 命名规范（例如 lowerCamelCase、下划线）。
/// 多数 Update* 方法的 updateServer 参数控制"本地状态更新后是否需要向服务器提交/广播"，具体行为由实现类决定。
/// </remarks>
public interface INetworkPlayer : IPlayerIdentity, IPlayerBattleState, IPlayerResources, IPlayerNetworkSync
{
}
