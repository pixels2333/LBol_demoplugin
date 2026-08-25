using System;
using System.Collections.Generic;
using System.Text.Json;
using NetworkPlugin.Network.Event;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class EnemyAttackVisualSyncTests
{
    [Fact]
    public void EnemyAttackVisualEvent_CreatesAndParsesCorrectly()
    {
        // 构造敌人攻击视觉数据（包括闪避状态）
        Dictionary<string, object> attackVisualData = new()
        {
            ["PlayerId"] = "player_123",
            ["UserName"] = "Tester",
            ["Timestamp"] = DateTime.Now.Ticks,
            ["EnemyId"] = "FairyRed",
            ["EnemyName"] = "红妖精",
            ["GunName"] = "BulletRed",
            ["GunType"] = "Single",
            ["Damage"] = 15f,
            ["DamageType"] = "Attack",
            ["IsGrazed"] = true,
            ["IsAccuracy"] = false,
        };

        GameEvent gameEvent = GameEventManager.CreateEvent(
            NetworkMessageTypes.OnEnemyAttackPlayerVisual,
            "Tester",
            attackVisualData
        );

        Assert.Equal(NetworkMessageTypes.OnEnemyAttackPlayerVisual, gameEvent.EventType);
        Assert.NotNull(gameEvent.Data);

        // 验证 JSON 反序列化与字段提取
        bool parsed = NetworkEventHelper.TryGetJsonElement(gameEvent.Data, out JsonElement root);
        Assert.True(parsed);

        Assert.Equal("player_123", NetworkEventHelper.GetString(root, "PlayerId"));
        Assert.Equal("FairyRed", NetworkEventHelper.GetString(root, "EnemyId"));
        Assert.Equal("BulletRed", NetworkEventHelper.GetString(root, "GunName"));
        Assert.Equal("Single", NetworkEventHelper.GetString(root, "GunType"));
        Assert.True(NetworkEventHelper.GetBool(root, "IsGrazed"));
        Assert.False(NetworkEventHelper.GetBool(root, "IsAccuracy"));
        Assert.True(NetworkEventHelper.TryGetInt(root, "Damage", out int dmg));
        Assert.Equal(15, dmg);
    }
}
