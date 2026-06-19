using System;
using AiSimClient.Network;

namespace AiSimClient.Actions;

// 交易动作发送器：对齐 TradeSyncPatch.cs L186-L235 的四个请求消息。
public class TradeSender
{
    private readonly SimNetworkClient _client;
    private static readonly Random Rng = new();

    public TradeSender(SimNetworkClient client) => _client = client;

    // 发起交易：OnTradeStartRequest。
    public void RequestStart(string playerAId, string playerBId, int maxSlots = 3)
    {
        var payload = new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = Guid.NewGuid().ToString("N"),
            TradeId = Guid.NewGuid().ToString("N"),
            PlayerAId = playerAId,
            PlayerBId = playerBId,
            MaxTradeSlots = maxSlots,
        };
        _client.SendGameEventData("OnTradeStartRequest", payload);
    }

    // 改价：OnTradeOfferUpdateRequest。外部程序无真实卡牌，Offer 传空数组。
    public void RequestOfferUpdate(string tradeId, string requesterPlayerId, int money)
    {
        var payload = new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = Guid.NewGuid().ToString("N"),
            TradeId = tradeId,
            RequesterPlayerId = requesterPlayerId,
            Offer = Array.Empty<object>(),
            Money = Math.Max(0, money),
            Exhibits = Array.Empty<string>(),
        };
        _client.SendGameEventData("OnTradeOfferUpdateRequest", payload);
    }

    // 确认：OnTradeConfirmRequest。
    public void RequestConfirm(string tradeId, string requesterPlayerId)
    {
        var payload = new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = Guid.NewGuid().ToString("N"),
            TradeId = tradeId,
            RequesterPlayerId = requesterPlayerId,
        };
        _client.SendGameEventData("OnTradeConfirmRequest", payload);
    }

    // 取消：OnTradeCancelRequest。
    public void RequestCancel(string tradeId, string requesterPlayerId)
    {
        var payload = new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = Guid.NewGuid().ToString("N"),
            TradeId = tradeId,
            RequesterPlayerId = requesterPlayerId,
        };
        _client.SendGameEventData("OnTradeCancelRequest", payload);
    }

    // 随机执行交易全流程（Start→Offer→Confirm）。
    public void RunRandomFlow(string selfId, string targetId)
    {
        string tradeId = Guid.NewGuid().ToString("N");
        RequestStart(selfId, targetId);
        RequestOfferUpdate(tradeId, selfId, Rng.Next(0, 50));
        RequestConfirm(tradeId, selfId);
    }
}