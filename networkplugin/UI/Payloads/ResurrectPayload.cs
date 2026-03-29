using NetworkPlugin.UI.Models;

namespace NetworkPlugin.UI.Payloads;

/// <summary>
/// Gap 支援面板数据类。
/// </summary>
public class ResurrectPayload
{
	/// <summary>可供支援/治疗的玩家列表</summary>
	public System.Collections.Generic.List<DeadPlayerEntry> Players { get; set; }

	/// <summary>是否允许取消操作</summary>
	public bool CanCancel { get; set; } = true;

	/// <summary>动作数值计算函数（兼容历史面板结构）</summary>
	public System.Func<int, int> CostCalculator { get; set; }
}
