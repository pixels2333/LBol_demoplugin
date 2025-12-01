using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// LBoL卡牌工具类 - 用于处理卡牌相关的操作和状态
    /// </summary>
    public static class CardUtils
    {
        /// <summary>
        /// 获取卡牌的详细信息
        /// </summary>
        /// <param name="card">卡牌实例</param>
        /// <returns>卡牌信息对象</returns>
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
                    Cost = ManaUtils.ManaGroupToArray(card.ManaGroup),
                    Damage = card.Config?.Damage1 ?? 0,
                    Block = card.Config?.Block1 ?? 0,
                    Upgraded = card.IsUpgraded,
                    Description = card.Config?.Description ?? "",
                    Keywords = card.Config?.Keywords ?? [],
                    TargetType = card.Config?.TargetType.ToString() ?? "Unknown",
                    Color = card.Config?.Color.ToString() ?? "Unknown"
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting card info: {ex.Message}");
                return new { Error = "Failed to get card info" };
            }
        } // 获取卡牌的详细信息对象，包含ID、名称、类型、稀有度、费用等属性

        /// <summary>
        /// 获取手牌中所有卡牌的信息
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>手牌信息列表</returns>
        public static List<object> GetHandCardsInfo(PlayerUnit player)
        {
            var handCards = new List<object>();

            try
            {
                if (player?.HandZone == null)
                {
                    return handCards;
                }

                foreach (var card in player.HandZone)
                {
                    handCards.Add(GetCardInfo(card));
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting hand cards info: {ex.Message}");
            }

            return handCards;
        } // 遍历玩家手牌区域，收集所有手牌的详细信息并返回列表

        /// <summary>
        /// 获取抽牌堆中所有卡牌的信息
        /// </summary>
        /// <param name="battle">战斗控制器</param>
        /// <returns>抽牌堆信息列表</returns>
        public static List<object> GetDrawDeckInfo(GameRunController gameRun)
        {
            var drawDeck = new List<object>();

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
        } // 遍历游戏运行基础的牌组，收集所有卡牌的详细信息并返回列表

        /// <summary>
        /// 获取弃牌堆中所有卡牌的信息
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>弃牌堆信息列表</returns>
        public static List<object> GetDiscardPileInfo(PlayerUnit player)
        {
            var discardPile = new List<object>();

            try
            {
                if (player?.DiscardZone == null)
                {
                    return discardPile;
                }

                foreach (var card in player.DiscardZone)
                {
                    discardPile.Add(GetCardInfo(card));
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting discard pile info: {ex.Message}");
            }

            return discardPile;
        } // 遍历玩家弃牌堆，收集所有弃置卡牌的详细信息并返回列表

        /// <summary>
        /// 获取卡牌区域信息
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>卡牌区域统计信息</returns>
        public static object GetCardZoneInfo(PlayerUnit player)
        {
            try
            {
                if (player == null)
                {
                    return new { Error = "Player is null" };
                }

                return new
                {
                    HandCount = player.HandZone?.Count ?? 0,
                    DrawCount = GetDrawDeckCount(player),
                    DiscardCount = player.DiscardZone?.Count ?? 0,
                    ExileCount = player.ExileZone?.Count ?? 0,
                    DeckCount = GetDeckCount(player),
                    PurgedCount = GetPurgedCount(player),
                    UpgradedCount = GetUpgradedCardCount(player),
                    TemporaryCount = GetTemporaryCardCount(player),
                    Timestamp = DateTime.Now.Ticks
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting card zone info: {ex.Message}");
                return new { Error = "Failed to get card zone info" };
            }
        } // 获取玩家所有卡牌区域的统计信息，包含手牌、牌组、弃牌堆等数量统计

        /// <summary>
        /// 获取抽牌堆数量
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>抽牌堆数量</returns>
        public static int GetDrawDeckCount(PlayerUnit player)
        {
            try
            {
                if (player == null)
                {
                    return 0;
                }

                // 尝试从游戏运行状态获取抽牌堆
                var gameRun = GameStateUtils.GetCurrentGameRun();
                if (gameRun != null)
                {
                    var drawZoneProperty = gameRun.GetType().GetProperty("DrawZone");
                    if (drawZoneProperty != null)
                    {
                        var drawZone = drawZoneProperty.GetValue(gameRun);
                        if (drawZone != null)
                        {
                            var countProperty = drawZone.GetType().GetProperty("Count");
                            if (countProperty != null)
                            {
                                return (int?)(countProperty.GetValue(drawZone)) ?? 0;
                            }
                        }
                    }
                }

                // 备用方案：从玩家获取
                var drawProperty = player.GetType().GetProperty("DrawZone");
                if (drawProperty != null)
                {
                    var drawZone = drawProperty.GetValue(player);
                    if (drawZone != null)
                    {
                        var countProperty = drawZone.GetType().GetProperty("Count");
                        if (countProperty != null)
                        {
                            return (int?)(countProperty.GetValue(drawZone)) ?? 0;
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting draw deck count: {ex.Message}");
                return 0;
            }
        } // 获取抽牌堆数量，通过反射访问游戏状态获取实际数值

        /// <summary>
        /// 检查卡牌是否可以打出
        /// </summary>
        /// <param name="card">卡牌实例</param>
        /// <param name="player">玩家实例</param>
        /// <returns>如果可以打出返回true</returns>
        public static bool CanPlayCard(Card card, PlayerUnit player)
        {
            try
            {
                if (card == null || player == null)
                {
                    return false;
                }

                // 基本检查
                if (!card.CanUse || card.IsForbidden)
                {
                    return false;
                }

                // 检查是否在手牌区域
                if (card.Zone?.ToString() != "Hand")
                {
                    return false;
                }

                // 检查法力是否足够
                var battle = player.Battle;
                if (battle != null && battle.BattleMana != null)
                {
                    // 检查是否有足够的法力
                    var availableMana = ManaUtils.ManaGroupToArray(battle.BattleMana);
                    var requiredMana = ManaUtils.ManaGroupToArray(card.ManaGroup);

                    for (int i = 0; i < 4; i++)
                    {
                        if (availableMana[i] < requiredMana[i])
                        {
                            return false;
                        }
                    }
                }

                // 检查卡牌目标是否有效
                if (card.Config?.TargetType != null)
                {
                    var targetType = card.Config.TargetType;
                    if (targetType == "Enemy" && !HasValidEnemyTarget(card, player))
                    {
                        return false;
                    }

                    if (targetType == "Ally" && !HasValidAllyTarget(card, player))
                    {
                        return false;
                    }
                }

                // 检查特殊条件
                if (card.IsEthereal && !IsEtherealPlayable(card))
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error checking if card can be played: {ex.Message}");
                return false;
            }
        } // 综合检查卡牌是否可以打出，验证基础条件、法力消耗、目标有效性等

        /// <summary>
        /// 获取卡牌的完整状态快照
        /// </summary>
        /// <param name="card">卡牌实例</param>
        /// <returns>卡牌状态快照</returns>
        public static object GetCardStateSnapshot(Card card)
        {
            if (card == null)
            {
                return null;
            }

            try
            {
                return new
                {
                    BasicInfo = GetCardInfo(card),
                    CurrentManaCost = ManaUtils.ManaGroupToArray(card.ManaGroup),
                    TemporaryStats = new
                    {
                        IsEthereal = card.IsEthereal,
                        IsRetain = card.IsRetain,
                        IsAutoExhaust = card.IsAutoExhaust,
                        IsTemporarilyUnplayable = card.IsForbidden,
                        TemporaryCostReduction = GetTemporaryCostReduction(card),
                        TemporaryDamageBonus = GetTemporaryDamageBonus(card),
                        TemporaryBlockBonus = GetTemporaryBlockBonus(card)
                    },
                    Position = card.Zone?.ToString() ?? "Unknown",
                    Owner = card.Zone?.Owner?.Id?.ToString() ?? "Unknown",
                    IsTemporarilyModified = CheckIfCardIsTemporarilyModified(card),
                    Timestamp = DateTime.Now.Ticks
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting card state snapshot: {ex.Message}");
                return new { Error = "Failed to get card state snapshot", Timestamp = DateTime.Now.Ticks };
            }
        } // 获取卡牌的完整状态快照，包含基础信息、临时状态和位置信息

        // 新增的辅助方法

        /// <summary>
        /// 获取牌组数量
        /// </summary>
        private static int GetDeckCount(PlayerUnit player)
        {
            try
            {
                var gameRun = GameStateUtils.GetCurrentGameRun();
                if (gameRun != null)
                {
                    var deckZoneProperty = gameRun.GetType().GetProperty("DeckZone");
                    if (deckZoneProperty != null)
                    {
                        var deckZone = deckZoneProperty.GetValue(gameRun);
                        if (deckZone != null)
                        {
                            var countProperty = deckZone.GetType().GetProperty("Count");
                            if (countProperty != null)
                            {
                                return (int?)(countProperty.GetValue(deckZone)) ?? 0;
                            }
                        }
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取被清除的卡牌数量
        /// </summary>
        private static int GetPurgedCount(PlayerUnit player)
        {
            try
            {
                var gameRun = GameStateUtils.GetCurrentGameRun();
                if (gameRun != null)
                {
                    var purgedZoneProperty = gameRun.GetType().GetProperty("PurgedZone");
                    if (purgedZoneProperty != null)
                    {
                        var purgedZone = purgedZoneProperty.GetValue(gameRun);
                        if (purgedZone != null)
                        {
                            var countProperty = purgedZone.GetType().GetProperty("Count");
                            if (countProperty != null)
                            {
                                return (int?)(countProperty.GetValue(purgedZone)) ?? 0;
                            }
                        }
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取升级卡牌数量
        /// </summary>
        private static int GetUpgradedCardCount(PlayerUnit player)
        {
            try
            {
                var count = 0;

                // 检查手牌
                if (player.HandZone != null)
                {
                    foreach (var card in player.HandZone.Cards)
                    {
                        if (card?.IsUpgraded == true)
                        {
                            count++;
                        }
                    }
                }

                // 检查牌组
                var gameRun = GameStateUtils.GetCurrentGameRun();
                if (gameRun?.DeckZone != null)
                {
                    foreach (var card in gameRun.DeckZone.Cards)
                    {
                        if (card?.IsUpgraded == true)
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取临时卡牌数量
        /// </summary>
        private static int GetTemporaryCardCount(PlayerUnit player)
        {
            try
            {
                var count = 0;

                // 检查手牌中的临时卡牌（如虚幻、无色等）
                if (player.HandZone != null)
                {
                    foreach (var card in player.HandZone.Cards)
                    {
                        if (card != null && (card.IsEthereal || card.IsColorless))
                        {
                            count++;
                        }
                    }
                }

                return count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 检查是否有有效的敌人目标
        /// </summary>
        private static bool HasValidEnemyTarget(Card card, PlayerUnit player)
        {
            try
            {
                var battle = player.Battle;
                if (battle?.EnemyGroup != null)
                {
                    return battle.EnemyGroup.Units.Count > 0;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否有有效的友方目标
        /// </summary>
        private static bool HasValidAllyTarget(Card card, PlayerUnit player)
        {
            try
            {
                var battle = player.Battle;
                if (battle != null)
                {
                    // 检查玩家自己
                    if (card.Zone?.Owner == player && !player.IsDeadOrEscaped)
                    {
                        return true;
                    }

                    // 检查是否有其他友方单位
                    // 这里可以扩展检查人偶等其他友方单位
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查虚幻卡是否可以打出
        /// </summary>
        private static bool IsEtherealPlayable(Card card)
        {
            try
            {
                // 虚幻卡通常只能在回合中第一次打出
                // 这里可以实现具体的逻辑
                return true; // 暂时返回true
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取临时费用减免
        /// </summary>
        private static int GetTemporaryCostReduction(Card card)
        {
            try
            {
                // 检查卡牌是否有临时费用减免效果
                // 这里可以实现具体的逻辑
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取临时伤害加成
        /// </summary>
        private static int GetTemporaryDamageBonus(Card card)
        {
            try
            {
                // 检查卡牌是否有临时伤害加成效果
                // 这里可以实现具体的逻辑
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取临时格挡加成
        /// </summary>
        private static int GetTemporaryBlockBonus(Card card)
        {
            try
            {
                // 检查卡牌是否有临时格挡加成效果
                // 这里可以实现具体的逻辑
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 检查卡牌是否被临时修改
        /// </summary>
        private static bool CheckIfCardIsTemporarilyModified(Card card)
        {
            try
            {
                // 检查卡牌是否有任何临时修改效果
                return card.IsEthereal || card.IsRetain || card.IsAutoExhaust || card.IsForbidden;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}