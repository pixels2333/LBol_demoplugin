namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 交易面板数据类
/// 用于传递交易面板的初始化参数
/// </summary>
public class TradePayload
{
	/// <summary>玩家1的名称</summary>
	public string Player1Name { get; set; }

	/// <summary>玩家2的名称</summary>
	public string Player2Name { get; set; }

	/// <summary>是否允许取消交易</summary>
	public bool CanCancel { get; set; } = true;

	/// <summary>最大交易槽位数</summary>
	public int MaxTradeSlots { get; set; } = 5;
}
