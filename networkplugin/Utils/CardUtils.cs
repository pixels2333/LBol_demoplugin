using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace NetworkPlugin.Utils
{
        public static class CardUtils
    {
        public static object GetCardInfo(Card card)
        {
            if (card == null)
            {
                return null;
            }

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

        public static List<object> GetHandCardsInfo(PlayerUnit player)
        {
            List<object> handCards = [];
            var battle = player?.Battle;
            if (battle?.HandZone == null)
                return handCards;

            foreach (var card in battle.HandZone)
                handCards.Add(GetCardInfo(card));

            return handCards;
        }

        public static List<object> GetDrawDeckInfo(GameRunController gameRun)
        {
            List<object> drawDeck = [];
            if (gameRun?.BaseDeck == null)
                return drawDeck;

            foreach (var card in gameRun.BaseDeck)
                drawDeck.Add(GetCardInfo(card));

            return drawDeck;
        }

        public static object GetPlayerCardZonesSummary(PlayerUnit player)
        {
            var battle = player?.Battle;
            return new
            {
                HandCount = battle?.HandZone?.Count ?? 0,
            };
        }

        public static bool CanPlayCard(Card card, PlayerUnit player)
        {
            if (card == null || player?.Battle == null)
            {
                return false;
            }

            return card.Battle == player.Battle && card.Zone == CardZone.Hand;
        }
    }
}
