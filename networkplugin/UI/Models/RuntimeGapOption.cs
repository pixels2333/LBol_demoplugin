using LBoL.Base;
using LBoL.Core.GapOptions;

namespace NetworkPlugin.UI.Models;

/// <summary>
/// 运行时 GapOption 基类：提供 ID、显示名称、显示描述的基本实现
/// </summary>
internal abstract class RuntimeGapOption : GapOption
{
    protected RuntimeGapOption(string id, string displayName, string displayDescription)
    {
        Id = id;
        DisplayName = displayName ?? string.Empty;
        DisplayDescription = displayDescription ?? string.Empty;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string DisplayDescription { get; }

    public sealed override GapOptionType Type => (GapOptionType)(-1000);

    /// <summary>
    /// 获取基础描述（返回运行时提供的 DisplayDescription）
    /// </summary>
    protected override string GetBaseDescription()
        => DisplayDescription;
}

/// <summary>
/// 运行时交易 GapOption：在联机 GapOptions 面板中显示交易入口
/// </summary>
internal sealed class RuntimeTradeGapOption : RuntimeGapOption
{
    public RuntimeTradeGapOption()
        : base("Trade", "交易", "与其他玩家交易卡牌、道具、金币等物品")
    {
    }
}

/// <summary>
/// 运行时治疗 GapOption：在联机 GapOptions 面板中显示治疗其他玩家的入口
/// </summary>
internal sealed class RuntimeTreatGapOption : RuntimeGapOption
{
    public RuntimeTreatGapOption()
        : base("Treat", "治疗", "回复指定玩家20%的生命值")
    {
    }
}
