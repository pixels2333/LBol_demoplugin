using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// LBoL 卡牌工具类：为网络同步提供稳定的卡牌信息提取与简单统计。
    /// </summary>
    public static class CardUtils
    {
        public static object GetCardInfo(Card card)
        {
            if (card == null)
            {
                return null;
            }

            try
            {
                return new
                {
                    CardId = card.Id,
                    CardName = card.Name,
                    CardType = card.CardType.ToString(),
                    Rarity = card.Config?.Rarity.ToString() ?? "Unknown",

                    Cost = ManaUtils.ManaGroupToArray(card.Cost),

                    Damage = card.RawDamage,
                    Block = card.RawBlock,

                    Upgraded = card.IsUpgraded,
                    Description = card.Description ?? string.Empty,

                    Keywords = card.Config?.Keywords.ToString() ?? string.Empty,
                    TargetType = card.Config?.TargetType?.ToString() ?? "Unknown",
                    Colors = card.Config?.Colors?.Select(c => c.ToString()).ToArray() ?? Array.Empty<string>(),
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting card info: {ex.Message}");
                return new { Error = "Failed to get card info" };
            }
        }

        public static List<object> GetHandCardsInfo(PlayerUnit player)
        {
            List<object> handCards = [];

            try
            {
                var battle = player?.Battle;
                if (battle?.HandZone == null)
                {
                    return handCards;
                }

                foreach (var card in battle.HandZone)
                {
                    handCards.Add(GetCardInfo(card));
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting hand cards info: {ex.Message}");
            }

            return handCards;
        }

        public static List<object> GetDrawDeckInfo(GameRunController gameRun)
        {
            List<object> drawDeck = [];

            try
            {
                if (gameRun?.BaseDeck == null)
                {
                    return drawDeck;
                }

                foreach (var card in gameRun.BaseDeck)
                {
                    drawDeck.Add(GetCardInfo(card));
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting draw deck info: {ex.Message}");
            }

            return drawDeck;
        }

        public static object GetPlayerCardZonesSummary(PlayerUnit player)
        {
            try
            {
                var battle = player?.Battle;
                return new
                {
                    HandCount = battle?.HandZone?.Count ?? 0,
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting card zones summary: {ex.Message}");
                return new { Error = "Failed to get zones summary" };
            }
        }

        public static bool CanPlayCard(Card card, PlayerUnit player)
        {
            try
            {
                if (card == null || player?.Battle == null)
                {
                    return false;
                }

                return card.Battle == player.Battle && card.Zone == CardZone.Hand;
            }
            catch
            {
                return false;
            }
        }
    }
}

