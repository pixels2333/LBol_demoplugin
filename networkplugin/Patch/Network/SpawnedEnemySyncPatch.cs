using System.Runtime.CompilerServices;
using LBoL.Core.Units;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 已生成敌方单位的 SpawnId 管理工具
/// 在敌方单位与 SpawnId 之间建立弱引用映射，用于跨客户端追踪敌人生成实例
/// </summary>
public static class SpawnedEnemySyncPatch
{
    private sealed class SpawnIdBox
    {
        public string SpawnId;
    }

    private static readonly ConditionalWeakTable<EnemyUnit, SpawnIdBox> SpawnIdTable = new();

    /// <summary>
    /// 尝试获取指定敌方单位的 SpawnId
    /// </summary>
    /// <param name="enemy">敌方单位</param>
    /// <param name="spawnId">输出的 SpawnId</param>
    /// <returns>成功获取则返回 true</returns>
    public static bool TryGetSpawnId(EnemyUnit enemy, out string spawnId)
    {
        spawnId = null;
        if (enemy == null)
        {
            return false;
        }

        if (SpawnIdTable.TryGetValue(enemy, out SpawnIdBox box) && !string.IsNullOrEmpty(box.SpawnId))
        {
            spawnId = box.SpawnId;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 将 SpawnId 绑定到敌方单位
    /// </summary>
    /// <param name="enemy">敌方单位</param>
    /// <param name="spawnId">要绑定的 SpawnId</param>
    public static void BindSpawnId(EnemyUnit enemy, string spawnId)
    {
        if (enemy == null || string.IsNullOrEmpty(spawnId))
        {
            return;
        }

        SpawnIdBox box = SpawnIdTable.GetOrCreateValue(enemy);
        box.SpawnId = spawnId;
    }

    /// <summary>
    /// 根据敌方组信息和索引构建标准化的 SpawnId
    /// </summary>
    /// <param name="enemyGroupId">敌方组 ID</param>
    /// <param name="spawnIndex">生成索引</param>
    /// <param name="rootIndex">根索引</param>
    /// <param name="enemyId">敌方 ID</param>
    /// <returns>格式化的 SpawnId 字符串</returns>
    public static string BuildSpawnId(string enemyGroupId, int spawnIndex, int rootIndex, string enemyId)
    {
        return $"{enemyGroupId ?? "unknown"}:{spawnIndex}:{rootIndex}:{enemyId ?? "unknown"}";
    }
}
