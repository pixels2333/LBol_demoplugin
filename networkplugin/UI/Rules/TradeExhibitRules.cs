using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;

namespace NetworkPlugin.UI.Rules;

/// <summary>
/// Exhibit trading rules: only losable exhibits can be traded, and some exhibits may be blacklisted.
/// The blacklist is maintained via an enum whose member names equal Exhibit.Id.
/// </summary>
internal static class TradeExhibitRules
{
    /// <summary>
    /// 不可交易的展品枚举（成员名需等于 Exhibit.Id）
    /// </summary>
    public enum NonTradableExhibits
    {
        // Add non-tradable exhibits here. Enum member name must equal Exhibit.Id.
        // Example:
        // SomeExhibitId,
    }

    private static readonly HashSet<string> Blacklist = new(
        Enum.GetNames(typeof(NonTradableExhibits)),
        StringComparer.Ordinal);

    /// <summary>
    /// 判断展品是否在黑名单中
    /// </summary>
    public static bool IsBlacklisted(string exhibitId)
        => !string.IsNullOrWhiteSpace(exhibitId) && Blacklist.Contains(exhibitId);

    /// <summary>
    /// 判断展品是否可交易（可失去且不在黑名单中）
    /// </summary>
    public static bool IsTradable(Exhibit exhibit)
        => exhibit != null
           && exhibit.LosableType == ExhibitLosableType.Losable
           && !IsBlacklisted(exhibit.Id);
}
