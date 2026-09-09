using System;
using System.Collections.Generic;
using System.IO;
using LBoL.Core.SaveData;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Core;

/// <summary>
/// 多人专属存档管理器：负责管理 multiplayer_run{slot}.yaml 与 multiplayer_roster_{rootSeed}.json，
/// 严格与原版单人存档进行物理隔离。
/// </summary>
public static class MultiplayerSaveManager
{
    private static string? _overrideSaveDirectory;

    /// <summary>
    /// 设置存档存储目录覆盖（用于单元测试或自定义路径）。
    /// </summary>
    public static void SetOverrideSaveDirectory(string? dir) => _overrideSaveDirectory = dir;

    /// <summary>
    /// 获取当前存档目录路径。
    /// </summary>
    public static string GetSaveDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_overrideSaveDirectory))
        {
            return _overrideSaveDirectory!;
        }

        try
        {
            var folder = LBoL.Presentation.GameMaster.PlatformHandler?.GetSaveDataFolder();
            if (!string.IsNullOrWhiteSpace(folder))
            {
                return folder;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[MultiplayerSaveManager] PlatformHandler 获取存档路径失败: {ex.Message}");
        }

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "LBoL", "Saves");
    }

    public static string GetMultiplayerSaveFileName(int slot = 0) => $"multiplayer_run{slot}.yaml";

    public static string GetRosterFileName(ulong rootSeed) => $"multiplayer_roster_{rootSeed}.json";

    public static string GetMultiplayerSavePath(int slot = 0) => Path.Combine(GetSaveDirectory(), GetMultiplayerSaveFileName(slot));

    public static string GetRosterPath(ulong rootSeed) => Path.Combine(GetSaveDirectory(), GetRosterFileName(rootSeed));

    /// <summary>
    /// 检查是否存在有效的多人专属存档。
    /// </summary>
    public static bool HasMultiplayerSave(int slot = 0)
    {
        try
        {
            string path = GetMultiplayerSavePath(slot);
            return File.Exists(path) && new FileInfo(path).Length > 0;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MultiplayerSaveManager] 检查多人存档失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 读取多人专属存档。文件损坏或不存在时返回 null。
    /// </summary>
    public static GameRunSaveData? LoadMultiplayerSave(int slot = 0)
    {
        try
        {
            string path = GetMultiplayerSavePath(slot);
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] bytes = File.ReadAllBytes(path);
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            return SaveDataHelper.DeserializeGameRun(bytes);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MultiplayerSaveManager] 读取多人存档失败 ({slot}): {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 保存多人专属存档至独立的 multiplayer_run{slot}.yaml 文件中，绝不触碰单人存档。
    /// </summary>
    public static void SaveMultiplayerSave(GameRunSaveData data, int slot = 0)
    {
        if (data == null)
        {
            Plugin.Logger?.LogWarning("[MultiplayerSaveManager] 尝试保存空的 GameRunSaveData，已忽略。");
            return;
        }

        try
        {
            string dir = GetSaveDirectory();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = GetMultiplayerSavePath(slot);
            byte[] bytes = SaveDataHelper.SerializeGameRun(data, false);
            File.WriteAllBytes(path, bytes);
            Plugin.Logger?.LogInfo($"[MultiplayerSaveManager] 成功保存多人专属存档: {path} (Seed: {data.RootSeed}, Chara: {data.Player?.Name})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MultiplayerSaveManager] 保存多人专属存档失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除指定槽位的多人专属存档。
    /// </summary>
    public static void DeleteMultiplayerSave(int slot = 0)
    {
        try
        {
            string path = GetMultiplayerSavePath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Plugin.Logger?.LogInfo($"[MultiplayerSaveManager] 已删除多人专属存档: {path}");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MultiplayerSaveManager] 删除多人专属存档失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 删除指定 RootSeed 的历史对局成员名单（Roster）。
    /// </summary>
    public static void DeleteRoster(ulong rootSeed)
    {
        if (rootSeed == 0) return;
        try
        {
            string path = GetRosterPath(rootSeed);
            if (File.Exists(path))
            {
                File.Delete(path);
                Plugin.Logger?.LogInfo($"[MultiplayerSaveManager] 已删除历史 Roster 名单文件: {path}");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MultiplayerSaveManager] 删除 Roster 名单失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 在房主端持久化记录对局成员名单（Roster）。
    /// </summary>
    public static void RecordPlayerInRoster(ulong rootSeed, string playerName, string charaId)
    {
        if (rootSeed == 0 || string.IsNullOrWhiteSpace(playerName) || string.IsNullOrWhiteSpace(charaId))
        {
            return;
        }

        try
        {
            string dir = GetSaveDirectory();
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = GetRosterPath(rootSeed);
            Dictionary<string, string> roster = LoadRoster(rootSeed);
            roster[playerName] = charaId;

            string json = JsonCompat.Serialize(roster);
            File.WriteAllText(path, json);
            Plugin.Logger?.LogInfo($"[MultiplayerSaveManager] 记录 Roster 成功: Seed={rootSeed}, Player={playerName}, Chara={charaId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MultiplayerSaveManager] 记录 Roster 失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 在房主端持久化记录全房间玩家名单（包括房主与所有远程玩家）。
    /// </summary>
    public static void RecordRoomPlayers(
        ulong rootSeed,
        string? hostId,
        string? hostName,
        string? hostChara,
        IEnumerable<INetworkPlayer>? players)
    {
        if (rootSeed == 0) return;
        string actualHostChara = !string.IsNullOrWhiteSpace(hostChara) ? hostChara : "Reimu";
        if (!string.IsNullOrWhiteSpace(hostId))
        {
            RecordPlayerInRoster(rootSeed, hostId, actualHostChara);
        }
        if (!string.IsNullOrWhiteSpace(hostName) && !string.Equals(hostName, hostId, StringComparison.OrdinalIgnoreCase))
        {
            RecordPlayerInRoster(rootSeed, hostName, actualHostChara);
        }

        if (players != null)
        {
            foreach (var p in players)
            {
                if (p != null && !string.IsNullOrWhiteSpace(p.playerId))
                {
                    string pChara = !string.IsNullOrWhiteSpace(p.chara) ? p.chara : "Reimu";
                    RecordPlayerInRoster(rootSeed, p.playerId, pChara);
                    if (!string.IsNullOrWhiteSpace(p.userName) && !string.Equals(p.userName, p.playerId, StringComparison.OrdinalIgnoreCase))
                    {
                        RecordPlayerInRoster(rootSeed, p.userName, pChara);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 读取指定 RootSeed 的历史对局成员名单。
    /// </summary>
    public static Dictionary<string, string> LoadRoster(ulong rootSeed)
    {
        if (rootSeed == 0)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            string path = GetRosterPath(rootSeed);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var dict = JsonCompat.Deserialize<Dictionary<string, string>>(json);
                    if (dict != null)
                    {
                        return new Dictionary<string, string>(dict, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MultiplayerSaveManager] 读取 Roster 失败 (Seed {rootSeed}): {ex.Message}");
        }

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 查询指定玩家在 Roster 中的历史角色。
    /// </summary>
    public static bool TryGetRosterCharacter(ulong rootSeed, string playerName, out string? charaId)
    {
        charaId = null;
        if (rootSeed == 0 || string.IsNullOrWhiteSpace(playerName))
        {
            return false;
        }

        try
        {
            var roster = LoadRoster(rootSeed);
            return roster.TryGetValue(playerName, out charaId);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[MultiplayerSaveManager] 查询 Roster 异常: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 老玩家识别算法（双重校验）：结合本地多人存档 RootSeed 匹配与房主端 Roster 名单。
    /// 支持传入房主网络广播的 hostRoster（用于远端客机机身未持有房主磁盘 Roster 文件的情形）。
    /// </summary>
    public static bool IsOldPlayer(
        ulong hostRootSeed,
        ulong? clientLocalRootSeed,
        string? clientLocalCharaId,
        string? playerName,
        IReadOnlyDictionary<string, string>? hostRoster,
        out string? inheritedCharaId)
    {
        inheritedCharaId = null;
        if (hostRootSeed == 0)
        {
            return false;
        }

        // 1. 本地多人存档 Seed 匹配
        if (clientLocalRootSeed.HasValue && clientLocalRootSeed.Value == hostRootSeed)
        {
            inheritedCharaId = !string.IsNullOrWhiteSpace(clientLocalCharaId) ? clientLocalCharaId : null;
            if (inheritedCharaId == null && !string.IsNullOrWhiteSpace(playerName))
            {
                if (hostRoster != null && hostRoster.TryGetValue(playerName, out var rChara))
                {
                    inheritedCharaId = rChara;
                }
                else
                {
                    TryGetRosterCharacter(hostRootSeed, playerName, out inheritedCharaId);
                }
            }
            return true;
        }

        // 2. 房主持久化历史对局名单 (Roster) 匹配
        if (!string.IsNullOrWhiteSpace(playerName))
        {
            if (hostRoster != null && hostRoster.TryGetValue(playerName, out var rosterChara))
            {
                inheritedCharaId = rosterChara;
                return true;
            }

            if (TryGetRosterCharacter(hostRootSeed, playerName, out var localRosterChara))
            {
                inheritedCharaId = localRosterChara;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 老玩家识别算法：重载以兼容本地磁盘查询。
    /// </summary>
    public static bool IsOldPlayer(
        ulong hostRootSeed,
        ulong? clientLocalRootSeed,
        string? clientLocalCharaId,
        string? playerName,
        out string? inheritedCharaId)
        => IsOldPlayer(hostRootSeed, clientLocalRootSeed, clientLocalCharaId, playerName, null, out inheritedCharaId);

    /// <summary>
    /// 老玩家识别算法（便捷重载）：自动加载本地多人存档并结合房主网络 Roster 进行比对。
    /// </summary>
    public static bool IsOldPlayer(ulong hostRootSeed, string? playerName, IReadOnlyDictionary<string, string>? hostRoster, out string? inheritedCharaId)
    {
        var localSave = LoadMultiplayerSave();
        ulong? localSeed = localSave?.RootSeed;
        string? localChara = localSave?.Player?.Name;
        return IsOldPlayer(hostRootSeed, localSeed, localChara, playerName, hostRoster, out inheritedCharaId);
    }

    /// <summary>
    /// 老玩家识别算法（便捷重载）：自动加载本地多人存档进行比对。
    /// </summary>
    public static bool IsOldPlayer(ulong hostRootSeed, string? playerName, out string? inheritedCharaId)
        => IsOldPlayer(hostRootSeed, playerName, (IReadOnlyDictionary<string, string>?)null, out inheritedCharaId);
}
