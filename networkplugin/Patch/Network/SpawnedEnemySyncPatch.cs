using System.Runtime.CompilerServices;
using LBoL.Core.Units;

namespace NetworkPlugin.Patch.Network;

public static class SpawnedEnemySyncPatch
{
    private sealed class SpawnIdBox
    {
        public string SpawnId;
    }

    private static readonly ConditionalWeakTable<EnemyUnit, SpawnIdBox> SpawnIdTable = new();

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

        public static void BindSpawnId(EnemyUnit enemy, string spawnId)
    {
        if (enemy == null || string.IsNullOrEmpty(spawnId))
        {
            return;
        }

        SpawnIdBox box = SpawnIdTable.GetOrCreateValue(enemy);
        box.SpawnId = spawnId;
    }

        public static string BuildSpawnId(string enemyGroupId, int spawnIndex, int rootIndex, string enemyId)
    {
        return $"{enemyGroupId ?? "unknown"}:{spawnIndex}:{rootIndex}:{enemyId ?? "unknown"}";
    }
}
