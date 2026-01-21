using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// Exhibit trading rules: only losable exhibits can be traded, and some exhibits may be blacklisted.
/// The blacklist is maintained via an enum whose member names equal Exhibit.Id.
/// </summary>
internal static class TradeExhibitRules
{
    public enum NonTradableExhibits
    {
        // Add non-tradable exhibits here. Enum member name must equal Exhibit.Id.
        // Example:
        // SomeExhibitId,
    }

    private static readonly HashSet<string> Blacklist = new(
        Enum.GetNames(typeof(NonTradableExhibits)),
        StringComparer.Ordinal);

    public static bool IsBlacklisted(string exhibitId)
        => !string.IsNullOrWhiteSpace(exhibitId) && Blacklist.Contains(exhibitId);

    public static bool IsTradable(Exhibit exhibit)
        => exhibit != null
           && exhibit.LosableType == ExhibitLosableType.Losable
           && !IsBlacklisted(exhibit.Id);
}
