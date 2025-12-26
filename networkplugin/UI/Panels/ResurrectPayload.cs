namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 复活面板数据类
/// 用于传递复活面板的初始化参数
/// </summary>
public class ResurrectPayload
{
	/// <summary>要复活的玩家列表</summary>
	public System.Collections.Generic.List<DeadPlayerEntry> DeadPlayers { get; set; }

	/// <summary>是否允许取消复活</summary>
	public bool CanCancel { get; set; } = true;

	/// <summary>复活成本计算函数</summary>
	public System.Func<int, int> CostCalculator { get; set; }
}
