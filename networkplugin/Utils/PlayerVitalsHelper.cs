using System;
using System.Linq;
using LBoL.Core.Units;
using NetworkPlugin.Network.NetworkPlayer;
using NetworkPlugin.Patch.Network;
using NetworkPlugin.Patch.UI;

namespace NetworkPlugin.Utils;

public static class PlayerVitalsHelper
{
    public static int GetDefaultMaxHpForCharacter(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId)) return 80;
        return characterId switch
        {
            "Reimu" => 80,
            "Marisa" => 75,
            "Sakuya" => 80,
            "Cirno" => 70,
            "Koishi" => 95,
            "Youmu" => 80,
            _ => 80,
        };
    }

    public static bool TryResolveVitals(string playerId, INetworkPlayer networkPlayer, out int currentHp, out int maxHp)
    {
        currentHp = 0;
        maxHp = 0;

        if (string.IsNullOrWhiteSpace(playerId))
        {
            return false;
        }

        string selfId = NetworkIdentityTracker.GetSelfPlayerId();
        bool isSelf = string.Equals(playerId, selfId, StringComparison.Ordinal) || string.Equals(playerId, "__local__", StringComparison.Ordinal);
        if (isSelf)
        {
            var localPlayer = GameStateUtils.GetCurrentPlayer();
            if (localPlayer != null)
            {
                currentHp = Math.Max(0, localPlayer.Hp);
                maxHp = Math.Max(1, localPlayer.MaxHp);
                return true;
            }
        }

        if (GapOptionsPanel_Patch.IsVirtualAiSimulatedPlayer(playerId))
        {
            maxHp = 80;
            currentHp = 56;
            return true;
        }

        // 1. 优先读取 networkPlayer 中的实时网络同步值
        if (networkPlayer != null && networkPlayer.maxHP > 0)
        {
            currentHp = Math.Max(0, networkPlayer.HP);
            maxHp = networkPlayer.maxHP;
            return true;
        }

        // 2. 尝试读取回合边界/战斗最新快照
        if (TurnBoundaryReceivePatch.TryGetLastSnapshot(playerId, out var snapshot) && snapshot?.PlayerState != null && snapshot.PlayerState.MaxHealth > 0)
        {
            currentHp = Math.Max(0, snapshot.PlayerState.Health);
            maxHp = snapshot.PlayerState.MaxHealth;
            return true;
        }

        // 3. 从 OtherPlayersOverlay 玩家详细快照中查询对应角色的 ModelName / CharacterId
        string characterId = null;
        try
        {
            var detailedPlayers = OtherPlayersOverlayPatch.SnapshotPlayersDetailed();
            var match = detailedPlayers.FirstOrDefault(p => string.Equals(p.PlayerId, playerId, StringComparison.Ordinal));
            if (!string.IsNullOrWhiteSpace(match.CharacterId))
            {
                characterId = match.CharacterId;
            }
        }
        catch
        {
        }

        if (string.IsNullOrWhiteSpace(characterId) && networkPlayer != null && !string.IsNullOrWhiteSpace(networkPlayer.chara))
        {
            characterId = networkPlayer.chara;
        }

        int defaultMaxHp = GetDefaultMaxHpForCharacter(characterId);
        maxHp = defaultMaxHp;

        if (networkPlayer != null && networkPlayer.HP > 0)
        {
            currentHp = Math.Min(networkPlayer.HP, maxHp);
        }
        else
        {
            currentHp = maxHp;
        }

        return maxHp > 0;
    }
}
