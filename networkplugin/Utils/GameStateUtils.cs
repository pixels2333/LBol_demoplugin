using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Units;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// LBoL游戏状态工具类 - 用于获取各种游戏状态信息
    /// 提供静态方法来安全地获取游戏数据
    /// </summary>
    public static class GameStateUtils
    {
        /// <summary>
        /// 获取当前玩家实例
        /// </summary>
        /// <returns>当前玩家，如果无法获取则返回null</returns>
        public static PlayerUnit GetCurrentPlayer()
        {
            try
            {
                // 通过反射获取GameRunController实例和玩家
                var gameRunControllerType = typeof(LBoL.Core.GameRunController);
                var instanceProperty = gameRunControllerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var gameRun = instanceProperty?.GetValue(null);

                if (gameRun != null)
                {
                    var playerProperty = gameRun.GetType().GetProperty("Player");
                    return playerProperty?.GetValue(gameRun) as PlayerUnit;
                }

                return null;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting current player: {ex.Message}");
                return null;
            }
        } // 通过反射机制获取当前玩家实例，支持网络环境下的玩家获取

        /// <summary>
        /// 获取当前玩家ID
        /// </summary>
        /// <returns>玩家ID字符串</returns>
        public static string GetCurrentPlayerId()
        {
            var player = GetCurrentPlayer();
            return player?.Id?.ToString() ?? "unknown_player";
        } // 获取当前玩家的唯一标识符字符串

        /// <summary>
        /// 获取当前战斗控制器
        /// </summary>
        /// <returns>GameRunController实例，如果无法获取则返回null</returns>
        public static GameRunController GetCurrentGameRun()
        {
            try
            {
                // 通过反射获取GameRunController实例
                var gameRunControllerType = typeof(LBoL.Core.GameRunController);
                var instanceProperty = gameRunControllerType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                return instanceProperty?.GetValue(null) as GameRunController;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting current game run: {ex.Message}");
                return null;
            }
        } // 获取当前游戏运行控制器实例，负责管理整个游戏流程

        /// <summary>
        /// 检查是否在战斗中
        /// </summary>
        /// <returns>如果在战斗中返回true</returns>
        public static bool IsInBattle()
        {
            try
            {
                // 通过反射检查当前是否在战斗状态
                var gameRun = GetCurrentGameRun();
                if (gameRun != null)
                {
                    var battleProperty = gameRun.GetType().GetProperty("Battle");
                    var battle = battleProperty?.GetValue(gameRun);
                    return battle != null;
                }
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error checking battle state: {ex.Message}");
                return false;
            }
        } // 检查当前游戏是否处于战斗状态，通过反射检查Battle属性是否存在

        /// <summary>
        /// 获取当前战斗中的所有玩家
        /// </summary>
        /// <returns>玩家列表</returns>
        public static List<PlayerUnit> GetPlayersInBattle()
        {
            List<PlayerUnit> players = [];

            try
            {
                // 在LBoL中，主要玩家来自GameRun
                var gameRun = GetCurrentGameRun();
                if (gameRun?.Player != null)
                {
                    players.Add(gameRun.Player);
                }

                // 如果是多人模式，可能需要从网络管理器获取其他玩家
                // 这里可以扩展支持多人游戏的玩家获取逻辑
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting players in battle: {ex.Message}");
            }

            return players;
        }

        /// <summary>
        /// 获取当前战斗中的所有敌人
        /// </summary>
        /// <returns>敌人列表</returns>
        public static List<EnemyUnit> GetEnemiesInBattle()
        {
            List<EnemyUnit> enemies = [];

            try
            {
                // 通过反射获取战斗中的敌人
                var gameRun = GetCurrentGameRun();
                if (gameRun != null)
                {
                    var battleProperty = gameRun.GetType().GetProperty("Battle");
                    var battle = battleProperty?.GetValue(gameRun);

                    if (battle != null)
                    {
                        // 尝试获取敌人组
                        var enemyGroupProperty = battle.GetType().GetProperty("EnemyGroup");
                        var enemyGroup = enemyGroupProperty?.GetValue(battle);

                        if (enemyGroup != null)
                        {
                            // 获取存活的敌人单位
                            var unitsProperty = enemyGroup.GetType().GetProperty("Units");
                            IEnumerable units = unitsProperty?.GetValue(enemyGroup) as System.Collections.IEnumerable;

                            if (units != null)
                            {
                                foreach (var unit in units)
                                {
                                    if (unit is EnemyUnit enemyUnit)
                                    {
                                        // 检查敌人是否存活
                                        var isAliveProperty = enemyUnit.GetType().GetProperty("IsAlive");
                                        var isAlive = (bool?)(isAliveProperty?.GetValue(enemyUnit)) ?? true;

                                        if (isAlive)
                                        {
                                            enemies.Add(enemyUnit);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting enemies in battle: {ex.Message}");
            }

            return enemies;
        }

        /// <summary>
        /// 检查当前玩家是否是主机/房主
        /// </summary>
        /// <returns>如果是主机返回true</returns>
        public static bool IsHost()
        {
            try
            {
                // 从网络客户端获取主机状态
                var serviceProvider = ModService.ServiceProvider;
                if (serviceProvider != null)
                {
                    var networkClient = serviceProvider.GetService<NetworkPlugin.Network.Client.INetworkClient>();
                    return networkClient?.IsHost ?? false;
                }

                // 备用方案：检查本地GameRun是否为主机
                var gameRun = GetCurrentGameRun();
                if (gameRun != null)
                {
                    // 检查是否有标记为主机的属性
                    var isHostProperty = gameRun.GetType().GetProperty("IsHost");
                    if (isHostProperty != null)
                    {
                        return (bool?)(isHostProperty.GetValue(gameRun)) ?? false;
                    }
                }

                return true; // 默认假设单人游戏是主机
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error checking host status: {ex.Message}");
                return false;
            }
        } // 检查当前玩家是否为主机/房主，支持网络游戏的房主识别

        /// <summary>
        /// 获取当前游戏模式
        /// </summary>
        /// <returns>游戏模式字符串</returns>
        public static string GetCurrentGameMode()
        {
            try
            {
                var gameRun = GetCurrentGameRun();
                if (gameRun != null)
                {
                    // 检查是否有网络相关的游戏模式属性
                    var gameModeProperty = gameRun.GetType().GetProperty("GameMode");
                    if (gameModeProperty != null)
                    {
                        var gameMode = gameModeProperty.GetValue(gameRun);
                        return gameMode?.ToString() ?? "Unknown";
                    }
                }

                // 如果无法检测，默认为单人模式
                return "SinglePlayer";
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting game mode: {ex.Message}");
                return "Unknown";
            }
        }

        /// <summary>
        /// 获取当前关卡/地图状态
        /// </summary>
        /// <returns>地图状态信息</returns>
        public static object GetMapState()
        {
            try
            {
                var gameRun = GetCurrentGameRun();
                if (gameRun != null)
                {
                    // 获取地图信息
                    var gameMapProperty = gameRun.GetType().GetProperty("GameMap");
                    var gameMap = gameMapProperty?.GetValue(gameRun);

                    if (gameMap != null)
                    {
                        // 获取当前节点
                        var currentNodeProperty = gameMap.GetType().GetProperty("CurrentNode");
                        var currentNode = currentNodeProperty?.GetValue(gameMap);

                        // 获取关卡信息
                        var stageProperty = gameRun.GetType().GetProperty("CurrentStage");
                        var stage = stageProperty?.GetValue(gameRun);

                        return new
                        {
                            CurrentStage = stage?.ToString() ?? "Unknown",
                            StationType = currentNode?.GetType().Name ?? "Unknown",
                            NodeId = currentNode?.ToString() ?? "Unknown",
                            Timestamp = DateTime.Now.Ticks
                        };
                    }
                }

                return new
                {
                    CurrentStage = "Unknown",
                    StationType = "Unknown",
                    NodeId = "Unknown",
                    Timestamp = DateTime.Now.Ticks
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting map state: {ex.Message}");
                return new { Error = "Failed to get map state", Timestamp = DateTime.Now.Ticks };
            }
        } // 获取当前游戏地图状态，包含关卡信息、节点类型和节点标识符

        /// <summary>
        /// 安全地获取实体的属性
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="entity">游戏实体</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>属性值或默认值</returns>
        public static T SafeGetProperty<T>(GameEntity entity, string propertyName, T defaultValue = default(T))
        {
            try
            {
                if (entity == null)
                {
                    return defaultValue;
                }

                var property = entity.GetType().GetProperty(propertyName);
                return property?.GetValue(entity) is T value ? value : defaultValue;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting property {propertyName}: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 获取当前战斗控制器
        /// </summary>
        /// <returns>BattleController实例，如果无法获取则返回null</returns>
        public static object GetCurrentBattle()
        {
            try
            {
                var gameRun = GetCurrentGameRun();
                if (gameRun != null)
                {
                    var battleProperty = gameRun.GetType().GetProperty("Battle");
                    return battleProperty?.GetValue(gameRun);
                }
                return null;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting current battle: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取玩家能量（Power）
        /// </summary>
        /// <param name="player">玩家单位</param>
        /// <returns>玩家能量值</returns>
        public static int GetPlayerPower(PlayerUnit player)
        {
            try
            {
                if (player == null)
                {
                    return 0;
                }

                // 尝试获取Power属性
                var powerProperty = player.GetType().GetProperty("Power");
                if (powerProperty != null)
                {
                    return (int?)(powerProperty.GetValue(player)) ?? 0;
                }

                // 备用方案：检查其他可能的能量相关属性
                var powerProperties = new[] { "Power", "Energy", "AttackPower", "TotalPower" };
                foreach (var propName in powerProperties)
                {
                    var prop = player.GetType().GetProperty(propName);
                    if (prop != null && prop.PropertyType == typeof(int))
                    {
                        return (int)(prop.GetValue(player) ?? 0);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting player power: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 获取当前法力（ManaGroup）
        /// </summary>
        /// <param name="player">玩家单位</param>
        /// <returns>法力组</returns>
        public static object GetCurrentMana(PlayerUnit player)
        {
            try
            {
                if (player == null)
                {
                    return CreateEmptyManaGroup();
                }

                // 尝试从战斗中获取法力
                var battleProperty = player.GetType().GetProperty("Battle");
                var battle = battleProperty?.GetValue(player);

                if (battle != null)
                {
                    var battleManaProperty = battle.GetType().GetProperty("BattleMana");
                    var battleMana = battleManaProperty?.GetValue(battle);

                    if (battleMana != null)
                    {
                        return battleMana;
                    }
                }

                // 备用方案：检查玩家的法力相关属性
                var manaProperties = new[] { "Mana", "CurrentMana", "BattleMana", "ManaGroup" };
                foreach (var propName in manaProperties)
                {
                    var prop = player.GetType().GetProperty(propName);
                    if (prop != null)
                    {
                        return prop.GetValue(player);
                    }
                }

                return CreateEmptyManaGroup();
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[GameStateUtils] Error getting current mana: {ex.Message}");
                return CreateEmptyManaGroup();
            }
        }

        /// <summary>
        /// 创建空的法力组
        /// </summary>
        /// <returns>空的法力组</returns>
        private static object CreateEmptyManaGroup()
        {
            try
            {
                // 尝试创建ManaGroup实例
                var manaGroupType = typeof(LBoL.Base.ManaGroup);
                if (manaGroupType != null)
                {
                    var constructor = manaGroupType.GetConstructor([]);
                    return constructor?.Invoke([]);
                }

                return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };
            }
            catch
            {
                return new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 };
            }
        }
    }
}