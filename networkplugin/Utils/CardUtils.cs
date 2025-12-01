using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// LBoL卡牌工具类
    /// 提供卡牌相关的操作、状态查询和数据转换功能
    /// 主要用于网络同步中的卡牌状态数据收集和验证
    /// </summary>
    /// <remarks>
    /// 这个工具类封装了LBoL游戏卡牌系统的复杂逻辑，为网络同步提供统一的卡牌信息接口。
    /// 主要功能包括：
    /// - 卡牌基本信息提取和序列化
    /// - 卡牌区域状态统计（手牌、牌库、弃牌堆等）
    /// - 卡牌可用性检查和目标验证
    /// - 临时状态和修改效果检测
    /// </remarks>
    public static class CardUtils
    {
        /// <summary>
        /// 获取卡牌的详细信息
        /// 提取卡牌的所有基本属性，用于网络传输和状态同步
        /// </summary>
        /// <param name="card">要分析的卡牌实例</param>
        /// <returns>包含卡牌详细信息的对象，如果卡牌为null则返回null</returns>
        public static object GetCardInfo(Card card)
        {
            // 验证卡牌实例是否有效
            if (card == null)
            {
                return null;
            }

            try
            {
                // 构建包含所有卡牌基本属性的信息对象
                return new
                {
                    // 基本标识信息
                    CardId = card.Id,                              // 卡牌唯一标识符
                    CardName = card.Name,                          // 卡牌显示名称

                    // 卡牌类型和属性
                    CardType = card.CardType.ToString(),           // 卡牌类型（攻击、技能、能力等）
                    Rarity = card.Config?.Rarity.ToString() ?? "Unknown", // 稀有度

                    // 法力消耗信息
                    Cost = ManaUtils.ManaGroupToArray(card.ManaGroup), // 四色法力消耗数组

                    // 基础数值
                    Damage = card.Config?.Damage1 ?? 0,            // 基础伤害值
                    Block = card.Config?.Block1 ?? 0,              // 基础格挡值

                    // 状态和属性
                    Upgraded = card.IsUpgraded,                    // 是否已升级
                    Description = card.Config?.Description ?? "",    // 卡牌描述文本

                    // 关键词和目标信息
                    Keywords = card.Config?.Keywords ?? [],        // 卡牌关键词列表
                    TargetType = card.Config?.TargetType.ToString() ?? "Unknown", // 目标类型
                    Color = card.Config?.Color.ToString() ?? "Unknown" // 卡牌颜色
                };
            }
            catch (Exception ex)
            {
                // 记录错误并返回错误信息对象
                Plugin.Logger?.LogError($"[CardUtils] Error getting card info: {ex.Message}");
                return new { Error = "Failed to get card info" };
            }
        } // 获取卡牌的详细信息对象，包含ID、名称、类型、稀有度、费用等属性

        /// <summary>
        /// 获取手牌中所有卡牌的信息
        /// 遍历玩家手牌区域，收集所有手牌的详细信息
        /// </summary>
        /// <param name="player">要检查手牌的玩家实例</param>
        /// <returns>包含手牌中所有卡牌信息的列表，如果玩家或手牌区域为空则返回空列表</returns>
        public static List<object> GetHandCardsInfo(PlayerUnit player)
        {
            var handCards = new List<object>();

            try
            {
                // 验证玩家和手牌区域是否有效
                if (player?.HandZone == null)
                {
                    return handCards; // 返回空列表而不是null
                }

                // 遍历手牌中的每张卡牌
                foreach (var card in player.HandZone)
                {
                    handCards.Add(GetCardInfo(card));
                }
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出异常，保持方法稳定性
                Plugin.Logger?.LogError($"[CardUtils] Error getting hand cards info: {ex.Message}");
            }

            return handCards;
        } // 遍历玩家手牌区域，收集所有手牌的详细信息并返回列表

        /// <summary>
        /// 获取抽牌堆中所有卡牌的信息
        /// 遍历游戏运行状态中的基础牌库，收集所有待抽取卡牌的信息
        /// </summary>
        /// <param name="gameRun">游戏运行控制器实例</param>
        /// <returns>包含抽牌堆中所有卡牌信息的列表，如果游戏运行状态或牌库为空则返回空列表</returns>
        public static List<object> GetDrawDeckInfo(GameRunController gameRun)
        {
            var drawDeck = new List<object>();

            try
            {
                // 验证游戏运行状态和基础牌库是否有效
                if (gameRun?.BaseDeck == null)
                {
                    return drawDeck;
                }

                // 遍历基础牌库中的每张卡牌
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
        /// 遍历玩家弃牌区域，收集所有已使用卡牌的信息
        /// </summary>
        /// <param name="player">要检查弃牌堆的玩家实例</param>
        /// <returns>包含弃牌堆中所有卡牌信息的列表，如果玩家或弃牌区域为空则返回空列表</returns>
        public static List<object> GetDiscardPileInfo(PlayerUnit player)
        {
            var discardPile = new List<object>();

            try
            {
                // 验证玩家和弃牌区域是否有效
                if (player?.DiscardZone == null)
                {
                    return discardPile;
                }

                // 遍历弃牌堆中的每张卡牌
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
        /// 获取卡牌区域的完整统计信息
        /// 收集玩家所有卡牌区域的数量统计和特殊卡牌计数
        /// </summary>
        /// <param name="player">要检查的玩家实例</param>
        /// <returns>包含所有卡牌区域统计信息的对象，如果玩家为null则返回错误对象</returns>
        public static object GetCardZoneInfo(PlayerUnit player)
        {
            try
            {
                if (player == null)
                {
                    return new { Error = "Player is null" };
                }

                // 构建包含所有卡牌区域统计信息的数据对象
                return new
                {
                    // 基本区域数量
                    HandCount = player.HandZone?.Count ?? 0,          // 手牌数量
                    DrawCount = GetDrawDeckCount(player),              // 抽牌堆数量
                    DiscardCount = player.DiscardZone?.Count ?? 0,    // 弃牌堆数量
                    ExileCount = player.ExileZone?.Count ?? 0,        // 放逐堆数量

                    // 牌库相关计数
                    DeckCount = GetDeckCount(player),                  // 牌库总数
                    PurgedCount = GetPurgedCount(player),              // 被清除卡牌数量

                    // 特殊卡牌计数
                    UpgradedCount = GetUpgradedCardCount(player),      // 升级卡牌数量
                    TemporaryCount = GetTemporaryCardCount(player),    // 临时卡牌数量

                    // 元数据
                    Timestamp = DateTime.Now.Ticks                     // 统计时间戳
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting card zone info: {ex.Message}");
                return new { Error = "Failed to get card zone info" };
            }
        } // 获取玩家所有卡牌区域的统计信息，包含手牌、牌组、弃牌堆等数量统计

        /// <summary>
        /// 获取抽牌堆的卡牌数量
        /// 通过反射访问游戏运行状态，获取当前抽牌堆中的卡牌数量
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>抽牌堆中的卡牌数量，如果无法获取则返回0</returns>
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
                    // 使用反射获取DrawZone属性
                    var drawZoneProperty = gameRun.GetType().GetProperty("DrawZone");
                    if (drawZoneProperty != null)
                    {
                        var drawZone = drawZoneProperty.GetValue(gameRun);
                        if (drawZone != null)
                        {
                            // 获取Count属性值
                            var countProperty = drawZone.GetType().GetProperty("Count");
                            if (countProperty != null)
                            {
                                return (int?)(countProperty.GetValue(drawZone)) ?? 0;
                            }
                        }
                    }
                }

                // 备用方案：尝试从玩家实例获取
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

                return 0; // 所有方法都失败时返回0
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting draw deck count: {ex.Message}");
                return 0;
            }
        } // 获取抽牌堆数量，通过反射访问游戏状态获取实际数值

        /// <summary>
        /// 检查卡牌是否可以被当前玩家打出
        /// 综合检查卡牌的可用性、法力消耗、目标有效性等条件
        /// </summary>
        /// <param name="card">要检查的卡牌实例</param>
        /// <param name="player">当前玩家实例</param>
        /// <returns>如果卡牌可以被打出返回true，否则返回false</returns>
        public static bool CanPlayCard(Card card, PlayerUnit player)
        {
            try
            {
                // 基本参数验证
                if (card == null || player == null)
                {
                    return false;
                }

                // 检查卡牌基本可用性
                if (!card.CanUse || card.IsForbidden)
                {
                    return false; // 卡牌被禁止使用
                }

                // 检查卡牌是否在手牌区域
                if (card.Zone?.ToString() != "Hand")
                {
                    return false; // 不在手牌中无法打出
                }

                // 检查法力是否足够
                var battle = player.Battle;
                if (battle != null && battle.BattleMana != null)
                {
                    // 获取可用法力和所需法力
                    var availableMana = ManaUtils.ManaGroupToArray(battle.BattleMana);
                    var requiredMana = ManaUtils.ManaGroupToArray(card.ManaGroup);

                    // 逐个检查四种颜色的法力是否足够
                    for (int i = 0; i < 4; i++)
                    {
                        if (availableMana[i] < requiredMana[i])
                        {
                            return false; // 法力不足
                        }
                    }
                }

                // 检查卡牌目标是否有效
                if (card.Config?.TargetType != null)
                {
                    var targetType = card.Config.TargetType;

                    // 如果需要敌人目标
                    if (targetType.ToString() == "Enemy" && !HasValidEnemyTarget(card, player))
                    {
                        return false; // 没有有效的敌人目标
                    }

                    // 如果需要友方目标
                    if (targetType.ToString() == "Ally" && !HasValidAllyTarget(card, player))
                    {
                        return false; // 没有有效的友方目标
                    }
                }

                // 检查特殊条件（如虚幻卡的限制）
                if (card.IsEthereal && !IsEtherealPlayable(card))
                {
                    return false; // 虚幻卡使用条件不满足
                }

                return true; // 所有检查通过，可以打出
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error checking if card can be played: {ex.Message}");
                return false; // 出错时默认返回false
            }
        } // 综合检查卡牌是否可以打出，验证基础条件、法力消耗、目标有效性等

        /// <summary>
        /// 获取卡牌的完整状态快照
        /// 提供卡牌当前时刻的完整状态信息，包括临时修改效果
        /// </summary>
        /// <param name="card">要分析状态的卡牌实例</param>
        /// <returns>包含卡牌完整状态快照的对象，如果卡牌为null则返回null</returns>
        public static object GetCardStateSnapshot(Card card)
        {
            if (card == null)
            {
                return null;
            }

            try
            {
                // 构建包含所有状态信息的快照对象
                return new
                {
                    // 基本信息
                    BasicInfo = GetCardInfo(card),                    // 基本卡牌信息
                    CurrentManaCost = ManaUtils.ManaGroupToArray(card.ManaGroup), // 当前法力消耗

                    // 临时状态信息
                    TemporaryStats = new
                    {
                        IsEthereal = card.IsEthereal,                 // 是否为虚幻卡
                        IsRetain = card.IsRetain,                     // 是否保留到下回合
                        IsAutoExhaust = card.IsAutoExhaust,           // 是否自动消耗
                        IsTemporarilyUnplayable = card.IsForbidden,    // 是否临时禁止使用
                        TemporaryCostReduction = GetTemporaryCostReduction(card), // 临时费用减免
                        TemporaryDamageBonus = GetTemporaryDamageBonus(card),     // 临时伤害加成
                        TemporaryBlockBonus = GetTemporaryBlockBonus(card)       // 临时格挡加成
                    },

                    // 位置和所有权信息
                    Position = card.Zone?.ToString() ?? "Unknown",   // 所在区域
                    Owner = card.Zone?.Owner?.Id?.ToString() ?? "Unknown", // 拥有者ID

                    // 修改状态
                    IsTemporarilyModified = CheckIfCardIsTemporarilyModified(card), // 是否有临时修改

                    // 元数据
                    Timestamp = DateTime.Now.Ticks                    // 快照时间戳
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardUtils] Error getting card state snapshot: {ex.Message}");
                return new { Error = "Failed to get card state snapshot", Timestamp = DateTime.Now.Ticks };
            }
        } // 获取卡牌的完整状态快照，包含基础信息、临时状态和位置信息

        // ========================================
        // 私有辅助方法 - 用于复杂的统计和检查逻辑
        // ========================================

        /// <summary>
        /// 获取牌库中的卡牌数量
        /// 通过反射访问游戏运行状态获取牌库总数
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>牌库中的卡牌总数</returns>
        private static int GetDeckCount(PlayerUnit player)
        {
            try
            {
                var gameRun = GameStateUtils.GetCurrentGameRun();
                if (gameRun != null)
                {
                    // 使用反射获取DeckZone属性
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
                return 0; // 出错时返回0
            }
        }

        /// <summary>
        /// 获取被清除的卡牌数量
        /// 通过反射访问游戏运行状态获取被永久移除的卡牌数量
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>被清除的卡牌数量</returns>
        private static int GetPurgedCount(PlayerUnit player)
        {
            try
            {
                var gameRun = GameStateUtils.GetCurrentGameRun();
                if (gameRun != null)
                {
                    // 使用反射获取PurgedZone属性
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
        /// 获取升级卡牌的总数量
        /// 统计手牌和牌库中所有升级卡牌的数量
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>升级卡牌的总数量</returns>
        private static int GetUpgradedCardCount(PlayerUnit player)
        {
            try
            {
                var count = 0;

                // 检查手牌中的升级卡牌
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

                // 检查牌库中的升级卡牌
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
        /// 获取临时卡牌的数量
        /// 统计手牌中的虚幻卡、无色卡等临时卡牌数量
        /// </summary>
        /// <param name="player">玩家实例</param>
        /// <returns>临时卡牌的总数量</returns>
        private static int GetTemporaryCardCount(PlayerUnit player)
        {
            try
            {
                var count = 0;

                // 检查手牌中的临时卡牌
                if (player.HandZone != null)
                {
                    foreach (var card in player.HandZone.Cards)
                    {
                        // 检查是否为虚幻卡或无色卡
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
        /// 验证当前战场上是否存在可以作为目标的敌人单位
        /// </summary>
        /// <param name="card">要检查的卡牌实例</param>
        /// <param name="player">玩家实例</param>
        /// <returns>如果存在有效敌人目标返回true</returns>
        private static bool HasValidEnemyTarget(Card card, PlayerUnit player)
        {
            try
            {
                var battle = player.Battle;
                if (battle?.EnemyGroup != null)
                {
                    // 检查是否有存活的敌人
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
        /// 验证是否存在可以作为友方目标的单位（包括玩家自己）
        /// </summary>
        /// <param name="card">要检查的卡牌实例</param>
        /// <param name="player">玩家实例</param>
        /// <returns>如果存在有效友方目标返回true</returns>
        private static bool HasValidAllyTarget(Card card, PlayerUnit player)
        {
            try
            {
                var battle = player.Battle;
                if (battle != null)
                {
                    // 检查玩家自己是否可以作为目标
                    if (card.Zone?.Owner == player && !player.IsDeadOrEscaped)
                    {
                        return true;
                    }

                    // TODO: 检查其他人偶等友方单位
                    // 这里可以扩展检查其他友方单位的逻辑
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查虚幻卡是否可以被使用
        /// 虚幻卡通常有特殊的使用限制（如每回合第一次）
        /// </summary>
        /// <param name="card">要检查的虚幻卡</param>
        /// <returns>如果可以打出返回true</returns>
        private static bool IsEtherealPlayable(Card card)
        {
            try
            {
                // TODO: 实现虚幻卡的具体使用逻辑
                // 虚幻卡通常只能在回合中第一次打出，或者有其他限制
                // 需要根据LBoL的具体规则实现
                return true; // 暂时返回true，待完善
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取卡牌的临时费用减免
        /// 检查卡牌是否有来自其他效果的法力费用减免
        /// </summary>
        /// <param name="card">要检查的卡牌</param>
        /// <returns>临时费用减免数值</returns>
        private static int GetTemporaryCostReduction(Card card)
        {
            try
            {
                // TODO: 实现临时费用减免的检测逻辑
                // 需要检查卡牌上的临时效果和buff
                return 0; // 暂时返回0，待完善
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取卡牌的临时伤害加成
        /// 检查卡牌是否有来自其他效果的临时伤害提升
        /// </summary>
        /// <param name="card">要检查的卡牌</param>
        /// <returns>临时伤害加成数值</returns>
        private static int GetTemporaryDamageBonus(Card card)
        {
            try
            {
                // TODO: 实现临时伤害加成的检测逻辑
                // 需要检查玩家身上的伤害增益效果
                return 0; // 暂时返回0，待完善
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取卡牌的临时格挡加成
        /// 检查卡牌是否有来自其他效果的临时格挡提升
        /// </summary>
        /// <param name="card">要检查的卡牌</param>
        /// <returns>临时格挡加成数值</returns>
        private static int GetTemporaryBlockBonus(Card card)
        {
            try
            {
                // TODO: 实现临时格挡加成的检测逻辑
                // 需要检查玩家身上的格挡增益效果
                return 0; // 暂时返回0，待完善
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 检查卡牌是否有任何临时修改效果
        /// 综合检查卡牌的各种临时状态和修改效果
        /// </summary>
        /// <param name="card">要检查的卡牌</param>
        /// <returns>如果卡牌有临时修改返回true</returns>
        private static bool CheckIfCardIsTemporarilyModified(Card card)
        {
            try
            {
                // 检查各种临时状态标志
                return card.IsEthereal ||      // 虚幻状态
                       card.IsRetain ||        // 保留状态
                       card.IsAutoExhaust ||    // 自动消耗状态
                       card.IsForbidden;       // 禁止使用状态
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}