using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 地图/事件发送器：OnMapNodeEnter（RoomEntrySyncPatch.cs L88）
// 与 OnMapNodeVoteCast（MapNodeMarkSyncPatch.cs L640）。
public class MapSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    private static readonly string[] StationTypes = { "Battle", "Elite", "Event", "Shop", "Treasure", "Rest" };

    public MapSender(SimNetworkClient client) => _client = client;

    public void SendMapNodeEnter(int x, int y, int act, string stationType)
    {
        var payload = new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            PlayerId = _client.SelfPlayerId,
            Node = new
            {
                X = x,
                Y = y,
                Act = act,
                StationType = stationType,
                NodeType = "MapNode",
                Status = "Unvisited",
            },
            freeMove = false,
            forced = false,
        };
        _client.SendGameEventData("OnMapNodeEnter", payload);
    }

    // 发送 UpdatePlayerLocation 系统消息：更新服务器侧会话的位置元数据，
    // 触发 PlayerListUpdate 广播，让游戏内地图显示 Player_1 在指定位置。
    // OnMapNodeEnter 只是广播事件，不更新位置元数据；必须发 UpdatePlayerLocation 才行。
    public void SendUpdatePlayerLocation(int x, int y, int act, string stationType)
    {
        var payload = new
        {
            LocationX = x,
            LocationY = y,
            LocationName = stationType,
            LocationType = "MapNode",
            Stage = act,
        };
        _client.SendGameEventData("UpdatePlayerLocation", payload);
    }

    public void SendMapNodeVoteCast(int sourceAct, int sourceX, int sourceY, int act, int x, int y)
    {
        var payload = new
        {
            PlayerId = _client.SelfPlayerId,
            SourceAct = sourceAct,
            SourceX = sourceX,
            SourceY = sourceY,
            Act = act,
            X = x,
            Y = y,
            Timestamp = DateTime.Now.Ticks,
        };
        _client.SendGameEventData("OnMapNodeVoteCast", payload);
    }

    public void RunRandom()
    {
        int act = Rng.Next(1, 4);
        int x = Rng.Next(0, 7);
        int y = Rng.Next(0, 7);
        string station = StationTypes[Rng.Next(StationTypes.Length)];
        SendMapNodeEnter(x, y, act, station);
        // 同时发 UpdatePlayerLocation 更新服务器侧位置元数据，
        // 让游戏内地图显示 Player_1 在独立位置，不再"跟着"本地玩家。
        SendUpdatePlayerLocation(x, y, act, station);
    }
}