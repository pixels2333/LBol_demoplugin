using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;

namespace NetworkPlugin.UI.Rules;

internal static class TradeExhibitRules
{
        public enum NonTradableExhibits
    {

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
