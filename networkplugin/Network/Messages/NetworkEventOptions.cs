using LiteNetLib;

namespace NetworkPlugin.Network.Messages;

/// <summary>
/// 游戏网络事件发送选项，用于细粒度控制消息的分发行为、回环策略及投递方式。
/// </summary>
public struct NetworkEventOptions
{
    /// <summary>
    /// 是否在本地内存进行回环派发（触发本地 <c>OnGameEventReceived</c>）。
    /// 对于权威状态广播（如交易结算、治疗复活、回合切换等），应设为 true。
    /// </summary>
    public bool IncludeSelf { get; set; }

    /// <summary>
    /// 点对点定向发送的目标玩家 ID。若为 null 或空，则为全房间广播。
    /// </summary>
    public string TargetPlayerId { get; set; }

    /// <summary>
    /// 网络传输可靠性方式，默认为 <see cref="DeliveryMethod.ReliableOrdered"/>。
    /// </summary>
    public DeliveryMethod DeliveryMethod { get; set; }

    /// <summary>
    /// 状态同步广播预设：包含本地回环（<c>IncludeSelf = true</c>），可靠有序传输。
    /// 适用于：状态同步、权威裁决、交易结算、治疗生效、种子同步等。
    /// </summary>
    public static NetworkEventOptions StateBroadcast => new()
    {
        IncludeSelf = true,
        TargetPlayerId = null,
        DeliveryMethod = DeliveryMethod.ReliableOrdered
    };

    /// <summary>
    /// 动作同步广播预设：不包含本地回环（<c>IncludeSelf = false</c>），防回声，可靠有序传输。
    /// 适用于：单机玩家发起的动作/输入（本地已执行，只同步给远端表现）。
    /// </summary>
    public static NetworkEventOptions ActionBroadcast => new()
    {
        IncludeSelf = false,
        TargetPlayerId = null,
        DeliveryMethod = DeliveryMethod.ReliableOrdered
    };

    /// <summary>
    /// 定向点对点预设：定向发送给指定玩家，可靠有序传输。
    /// 若目标是本地玩家自身，底层将智能转为本地主线程回环。
    /// </summary>
    /// <param name="targetPlayerId">目标玩家 ID。</param>
    public static NetworkEventOptions Targeted(string targetPlayerId) => new()
    {
        IncludeSelf = false,
        TargetPlayerId = targetPlayerId,
        DeliveryMethod = DeliveryMethod.ReliableOrdered
    };
}
