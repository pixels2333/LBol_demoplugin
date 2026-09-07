using LBoL.Base;
using LBoL.Core.GapOptions;

namespace NetworkPlugin.UI.Models;

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

        protected override string GetBaseDescription()
        => DisplayDescription;
}

internal sealed class RuntimeTradeGapOption : RuntimeGapOption
{
    public RuntimeTradeGapOption()
        : base("Trade", "交易", "与其他玩家交易卡牌、道具、金币等物品")
    {
    }
}

internal sealed class RuntimeTreatGapOption : RuntimeGapOption
{
    public RuntimeTreatGapOption()
        : base("Treat", "治疗", "回复指定玩家20%的生命值")
    {
    }
}
