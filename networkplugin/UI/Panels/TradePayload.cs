namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 交易面板数据类
/// 用于传递交易面板的初始化参数
/// </summary>
public class TradePayload
{
	/// <summary>
	/// 本次交易会话 ID（可选；为空时由 UI/同步层生成）。
	/// </summary>
	public string TradeId { get; set; }

	/// <summary>
	/// 玩家1的网络 PlayerId（可选；为空时默认使用本地玩家）。
	/// </summary>
	public string Player1Id { get; set; }

	/// <summary>
	/// 玩家2的网络 PlayerId（可选；为空时将自动选择一个远端玩家）。
	/// </summary>
	public string Player2Id { get; set; }

	/// <summary>玩家1的名称</summary>
	public string Player1Name { get; set; }

	/// <summary>玩家2的名称</summary>
	public string Player2Name { get; set; }

	/// <summary>是否允许取消交易</summary>
	public bool CanCancel { get; set; } = true;

	/// <summary>最大交易槽位数</summary>
	public int MaxTradeSlots { get; set; } = 5;
}
