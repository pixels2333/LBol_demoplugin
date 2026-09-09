using System;
using System.IO;
using FluentAssertions;
using LBoL.Core;
using LBoL.Core.SaveData;
using Moq;
using NetworkPlugin.Core;
using NetworkPlugin.Network.NetworkPlayer;
using Xunit;

namespace NetworkPlugin.Tests;

public class MultiplayerSaveManagerTests : IDisposable
{
    private readonly string _testDir;

    public MultiplayerSaveManagerTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "LBoL_MultiplayerSaveTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
        MultiplayerSaveManager.SetOverrideSaveDirectory(_testDir);
    }

    public void Dispose()
    {
        MultiplayerSaveManager.SetOverrideSaveDirectory(null);
        try
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Test Cleanup] 目录清理异常: {ex.Message}");
        }
    }

    private GameRunSaveData CreateSampleSaveData(ulong rootSeed = 123456789UL, string playerName = "Reimu")
    {
        return new GameRunSaveData
        {
            RootSeed = rootSeed,
            Difficulty = GameDifficulty.Normal,
            Mode = GameMode.FreeMode,
            Player = new PlayerSaveData
            {
                Name = playerName,
                Hp = 80,
                MaxHp = 80,
            },
            PlayedSeconds = 120,
            SaveTimestamp = "2026-09-08T10:00:00Z",
            GameVersion = "1.5.0",
            GameRevision = "1"
        };
    }

    [Fact]
    public void MultiplayerSaveManager_SaveAndLoad_ShouldMatchData()
    {
        var sample = CreateSampleSaveData(987654321UL, "Marisa");

        MultiplayerSaveManager.HasMultiplayerSave(0).Should().BeFalse();

        MultiplayerSaveManager.SaveMultiplayerSave(sample, 0);

        MultiplayerSaveManager.HasMultiplayerSave(0).Should().BeTrue();

        var loaded = MultiplayerSaveManager.LoadMultiplayerSave(0);
        loaded.Should().NotBeNull();
        loaded!.RootSeed.Should().Be(987654321UL);
        loaded.Difficulty.Should().Be(GameDifficulty.Normal);
        loaded.Player.Should().NotBeNull();
        loaded.Player.Name.Should().Be("Marisa");
    }

    [Fact]
    public void MultiplayerSaveManager_Delete_ShouldRemoveFile()
    {
        var sample = CreateSampleSaveData();
        MultiplayerSaveManager.SaveMultiplayerSave(sample, 0);
        MultiplayerSaveManager.HasMultiplayerSave(0).Should().BeTrue();

        MultiplayerSaveManager.DeleteMultiplayerSave(0);

        MultiplayerSaveManager.HasMultiplayerSave(0).Should().BeFalse();
        MultiplayerSaveManager.LoadMultiplayerSave(0).Should().BeNull();
    }

    [Fact]
    public void MultiplayerSaveManager_SinglePlayerIsolation_ShouldNotOverwriteNativeFile()
    {
        // 模拟原版单人存档文件
        string nativeGameSav = Path.Combine(_testDir, "game0.sav");
        string nativeRunYaml = Path.Combine(_testDir, "run0.yaml");
        File.WriteAllText(nativeGameSav, "NATIVE_GAME_SAVE_CONTENT");
        File.WriteAllText(nativeRunYaml, "NATIVE_RUN_YAML_CONTENT");

        var multiplayerSave = CreateSampleSaveData(55555UL, "Sakuya");

        // 保存多人存档
        MultiplayerSaveManager.SaveMultiplayerSave(multiplayerSave, 0);

        // 验证原版单人存档完全未被触碰或覆盖
        File.Exists(nativeGameSav).Should().BeTrue();
        File.ReadAllText(nativeGameSav).Should().Be("NATIVE_GAME_SAVE_CONTENT");
        File.Exists(nativeRunYaml).Should().BeTrue();
        File.ReadAllText(nativeRunYaml).Should().Be("NATIVE_RUN_YAML_CONTENT");

        // 删除多人存档
        MultiplayerSaveManager.DeleteMultiplayerSave(0);

        // 验证原版单人存档仍然完好无损
        File.Exists(nativeGameSav).Should().BeTrue();
        File.ReadAllText(nativeGameSav).Should().Be("NATIVE_GAME_SAVE_CONTENT");
    }

    [Fact]
    public void Roster_RecordAndQuery_ShouldRecognizeOldPlayers()
    {
        ulong seed = 88888888UL;

        MultiplayerSaveManager.RecordPlayerInRoster(seed, "PlayerAlice", "Cirno");
        MultiplayerSaveManager.RecordPlayerInRoster(seed, "PlayerBob", "Koishi");

        bool foundAlice = MultiplayerSaveManager.TryGetRosterCharacter(seed, "PlayerAlice", out var charaAlice);
        foundAlice.Should().BeTrue();
        charaAlice.Should().Be("Cirno");

        bool foundBob = MultiplayerSaveManager.TryGetRosterCharacter(seed, "PlayerBob", out var charaBob);
        foundBob.Should().BeTrue();
        charaBob.Should().Be("Koishi");

        // 大小写不敏感校验
        bool foundAliceLower = MultiplayerSaveManager.TryGetRosterCharacter(seed, "playeralice", out var charaAliceLower);
        foundAliceLower.Should().BeTrue();
        charaAliceLower.Should().Be("Cirno");
    }

    [Fact]
    public void Roster_UnknownPlayer_ShouldReturnNewPlayer()
    {
        ulong seed = 99999999UL;
        MultiplayerSaveManager.RecordPlayerInRoster(seed, "PlayerAlice", "Cirno");

        bool found = MultiplayerSaveManager.TryGetRosterCharacter(seed, "UnknownPlayer", out var chara);
        found.Should().BeFalse();
        chara.Should().BeNull();
    }

    [Fact]
    public void IsOldPlayer_WithMatchingSeed_ShouldRecognizeOldPlayer()
    {
        ulong hostSeed = 11223344UL;

        // 1. 本地种子匹配 -> 老玩家
        bool isOldBySeed = MultiplayerSaveManager.IsOldPlayer(
            hostRootSeed: hostSeed,
            clientLocalRootSeed: hostSeed,
            clientLocalCharaId: "Reimu",
            playerName: "AnyName",
            out var inheritedChara1
        );
        isOldBySeed.Should().BeTrue();
        inheritedChara1.Should().Be("Reimu");

        // 2. 本地种子不匹配，但命中房主端 Roster -> 老玩家
        MultiplayerSaveManager.RecordPlayerInRoster(hostSeed, "ReturningPlayer", "Marisa");
        bool isOldByRoster = MultiplayerSaveManager.IsOldPlayer(
            hostRootSeed: hostSeed,
            clientLocalRootSeed: 999999UL,
            clientLocalCharaId: null,
            playerName: "ReturningPlayer",
            out var inheritedChara2
        );
        isOldByRoster.Should().BeTrue();
        inheritedChara2.Should().Be("Marisa");

        // 3. 种子不匹配且不在 Roster -> 新玩家
        bool isNew = MultiplayerSaveManager.IsOldPlayer(
            hostRootSeed: hostSeed,
            clientLocalRootSeed: null,
            clientLocalCharaId: null,
            playerName: "BrandNewFriend",
            out var inheritedChara3
        );
        isNew.Should().BeFalse();
        inheritedChara3.Should().BeNull();
    }

    [Fact]
    public void CorruptedSaveFile_ShouldHandleGracefully()
    {
        string savePath = MultiplayerSaveManager.GetMultiplayerSavePath(0);
        File.WriteAllText(savePath, "THIS IS CORRUPTED NOT A VALID YAML SAVE");

        MultiplayerSaveManager.HasMultiplayerSave(0).Should().BeTrue();

        var loaded = MultiplayerSaveManager.LoadMultiplayerSave(0);
        loaded.Should().BeNull();
    }

    [Fact]
    public void MultiplayerSaveManager_NullData_ShouldNotThrowAndNotCreateFile()
    {
        MultiplayerSaveManager.SaveMultiplayerSave(null!, 0);
        MultiplayerSaveManager.HasMultiplayerSave(0).Should().BeFalse();
    }

    [Fact]
    public void MultiplayerSaveManager_MultipleSlots_ShouldBeIsolated()
    {
        var save0 = CreateSampleSaveData(100UL, "Reimu");
        var save1 = CreateSampleSaveData(200UL, "Marisa");

        MultiplayerSaveManager.SaveMultiplayerSave(save0, 0);
        MultiplayerSaveManager.SaveMultiplayerSave(save1, 1);

        MultiplayerSaveManager.HasMultiplayerSave(0).Should().BeTrue();
        MultiplayerSaveManager.HasMultiplayerSave(1).Should().BeTrue();

        var loaded0 = MultiplayerSaveManager.LoadMultiplayerSave(0);
        var loaded1 = MultiplayerSaveManager.LoadMultiplayerSave(1);

        loaded0!.RootSeed.Should().Be(100UL);
        loaded0.Player.Name.Should().Be("Reimu");

        loaded1!.RootSeed.Should().Be(200UL);
        loaded1.Player.Name.Should().Be("Marisa");

        MultiplayerSaveManager.DeleteMultiplayerSave(0);
        MultiplayerSaveManager.HasMultiplayerSave(0).Should().BeFalse();
        MultiplayerSaveManager.HasMultiplayerSave(1).Should().BeTrue();
    }

    [Fact]
    public void Roster_EmptyOrInvalidSeedOrName_ShouldReturnFalse()
    {
        MultiplayerSaveManager.RecordPlayerInRoster(0UL, "PlayerA", "Reimu");
        MultiplayerSaveManager.RecordPlayerInRoster(12345UL, "", "Reimu");
        MultiplayerSaveManager.RecordPlayerInRoster(12345UL, "PlayerA", "");

        MultiplayerSaveManager.TryGetRosterCharacter(0UL, "PlayerA", out var chara1).Should().BeFalse();
        chara1.Should().BeNull();

        MultiplayerSaveManager.TryGetRosterCharacter(12345UL, "", out var chara2).Should().BeFalse();
        chara2.Should().BeNull();
    }

    [Fact]
    public void Roster_UpdateExistingPlayer_ShouldOverwriteCharacter()
    {
        ulong seed = 777777UL;
        MultiplayerSaveManager.RecordPlayerInRoster(seed, "PlayerOne", "Reimu");
        MultiplayerSaveManager.TryGetRosterCharacter(seed, "PlayerOne", out var chara1);
        chara1.Should().Be("Reimu");

        MultiplayerSaveManager.RecordPlayerInRoster(seed, "PlayerOne", "Sakuya");
        MultiplayerSaveManager.TryGetRosterCharacter(seed, "PlayerOne", out var chara2);
        chara2.Should().Be("Sakuya");
    }

    [Fact]
    public void IsOldPlayer_ZeroHostSeed_ShouldReturnFalse()
    {
        bool isOld = MultiplayerSaveManager.IsOldPlayer(
            hostRootSeed: 0UL,
            clientLocalRootSeed: 0UL,
            clientLocalCharaId: "Reimu",
            playerName: "Host",
            out var inheritedChara
        );
        isOld.Should().BeFalse();
        inheritedChara.Should().BeNull();
    }

    [Fact]
    public void IsOldPlayer_WithRemoteHostRoster_ShouldRecognizeOldPlayer()
    {
        ulong hostSeed = 55667788UL;
        var hostRoster = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "RemoteAlice", "Cirno" },
            { "RemoteBob", "Koishi" }
        };

        // 1. 客机无本地存档，但存在于远端 hostRoster 中 -> 识别为老玩家并继承角色
        bool isOldAlice = MultiplayerSaveManager.IsOldPlayer(
            hostRootSeed: hostSeed,
            clientLocalRootSeed: null,
            clientLocalCharaId: null,
            playerName: "RemoteAlice",
            hostRoster: hostRoster,
            out var charaAlice
        );
        isOldAlice.Should().BeTrue();
        charaAlice.Should().Be("Cirno");

        // 2. 大小写不敏感比对
        bool isOldAliceLower = MultiplayerSaveManager.IsOldPlayer(
            hostRootSeed: hostSeed,
            clientLocalRootSeed: null,
            clientLocalCharaId: null,
            playerName: "remotealice",
            hostRoster: hostRoster,
            out var charaAliceLower
        );
        isOldAliceLower.Should().BeTrue();
        charaAliceLower.Should().Be("Cirno");

        // 3. 客机本地种子匹配，但 clientLocalCharaId 为空 -> 从 hostRoster 补全角色
        bool isOldBob = MultiplayerSaveManager.IsOldPlayer(
            hostRootSeed: hostSeed,
            clientLocalRootSeed: hostSeed,
            clientLocalCharaId: null,
            playerName: "RemoteBob",
            hostRoster: hostRoster,
            out var charaBob
        );
        isOldBob.Should().BeTrue();
        charaBob.Should().Be("Koishi");

        // 4. 不在 hostRoster 中且无匹配本地种子 -> 新玩家
        bool isNewCharlie = MultiplayerSaveManager.IsOldPlayer(
            hostRootSeed: hostSeed,
            clientLocalRootSeed: null,
            clientLocalCharaId: null,
            playerName: "Charlie",
            hostRoster: hostRoster,
            out var charaCharlie
        );
        isNewCharlie.Should().BeFalse();
        charaCharlie.Should().BeNull();
    }

    [Fact]
    public void IsOldPlayer_AutoLoadLocalSave_WithHostRoster_ShouldWorkCorrectly()
    {
        ulong hostSeed = 33445566UL;

        // 1. 无本地存档且不在 hostRoster -> 新玩家
        bool isNew = MultiplayerSaveManager.IsOldPlayer(hostSeed, "PlayerX", null, out var charaX);
        isNew.Should().BeFalse();
        charaX.Should().BeNull();

        // 2. 写入本地多人存档，种子匹配 -> 老玩家
        var sample = CreateSampleSaveData(hostSeed, "Sakuya");
        MultiplayerSaveManager.SaveMultiplayerSave(sample, 0);

        bool isOldByLocalSave = MultiplayerSaveManager.IsOldPlayer(hostSeed, "PlayerX", null, out var charaLocal);
        isOldByLocalSave.Should().BeTrue();
        charaLocal.Should().Be("Sakuya");

        // 3. 本地存档种子不匹配，但通过 hostRoster 命中 -> 老玩家
        var hostRoster = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "PlayerY", "Marisa" }
        };
        ulong differentHostSeed = 99887766UL; // 与本地存档 33445566 不一致
        bool isOldByHostRoster = MultiplayerSaveManager.IsOldPlayer(differentHostSeed, "PlayerY", hostRoster, out var charaY);
        isOldByHostRoster.Should().BeTrue();
        charaY.Should().Be("Marisa");

        // 清理本地存档
        MultiplayerSaveManager.DeleteMultiplayerSave(0);
    }

    [Fact]
    public void IsOldPlayer_NullOrEmptyInputs_ShouldHandleSafely()
    {
        ulong hostSeed = 12345UL;
        var hostRoster = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "PlayerA", "Reimu" }
        };

        // null 或空白玩家名
        MultiplayerSaveManager.IsOldPlayer(hostSeed, null, hostRoster, out var c1).Should().BeFalse();
        c1.Should().BeNull();

        MultiplayerSaveManager.IsOldPlayer(hostSeed, "  ", hostRoster, out var c2).Should().BeFalse();
        c2.Should().BeNull();

        // hostRootSeed 为 0
        MultiplayerSaveManager.IsOldPlayer(0UL, "PlayerA", hostRoster, out var c3).Should().BeFalse();
        c3.Should().BeNull();
    }

    [Fact]
    public void DeleteRoster_ShouldRemoveRosterFile()
    {
        ulong seed = 12345678UL;
        MultiplayerSaveManager.RecordPlayerInRoster(seed, "Alice", "Reimu");
        File.Exists(MultiplayerSaveManager.GetRosterPath(seed)).Should().BeTrue();

        MultiplayerSaveManager.DeleteRoster(seed);
        File.Exists(MultiplayerSaveManager.GetRosterPath(seed)).Should().BeFalse();
    }

    [Fact]
    public void RecordRoomPlayers_ShouldRecordHostAndAllPlayers()
    {
        ulong seed = 87654321UL;
        var p1 = new Mock<INetworkPlayer>();
        p1.SetupGet(p => p.playerId).Returns("P1_Id");
        p1.SetupGet(p => p.userName).Returns("P1_User");
        p1.SetupGet(p => p.chara).Returns("Marisa");

        var p2 = new Mock<INetworkPlayer>();
        p2.SetupGet(p => p.playerId).Returns("P2_Id");
        p2.SetupGet(p => p.userName).Returns("P2_Id"); // same username
        p2.SetupGet(p => p.chara).Returns("Cirno");

        MultiplayerSaveManager.RecordRoomPlayers(seed, "Host_Id", "Host_User", "Reimu", new[] { p1.Object, p2.Object });

        MultiplayerSaveManager.TryGetRosterCharacter(seed, "Host_Id", out var hostChara1).Should().BeTrue();
        hostChara1.Should().Be("Reimu");

        MultiplayerSaveManager.TryGetRosterCharacter(seed, "Host_User", out var hostChara2).Should().BeTrue();
        hostChara2.Should().Be("Reimu");

        MultiplayerSaveManager.TryGetRosterCharacter(seed, "P1_Id", out var p1Chara).Should().BeTrue();
        p1Chara.Should().Be("Marisa");

        MultiplayerSaveManager.TryGetRosterCharacter(seed, "P1_User", out var p1NameChara).Should().BeTrue();
        p1NameChara.Should().Be("Marisa");

        MultiplayerSaveManager.TryGetRosterCharacter(seed, "P2_Id", out var p2Chara).Should().BeTrue();
        p2Chara.Should().Be("Cirno");
    }
}

