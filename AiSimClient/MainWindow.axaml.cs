using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Threading;
using AiSimClient.Actions;
using AiSimClient.Network;

namespace AiSimClient;

public partial class MainWindow : Window
{
    private readonly SimNetworkClient _net = new();
    private readonly CardPlaySender _cardPlay;
    private readonly MidGameJoinSender _midGameJoin;
    private readonly TradeSender _trade;
    private readonly HealSender _heal;
    private readonly TurnSender _turn;
    private readonly DamageSender _damage;
    private readonly StatusEffectSender _status;
    private readonly ManaSender _mana;
    private readonly MapSender _map;
    private readonly EventSender _event;
    private readonly ResurrectSender _resurrect;
    private readonly ObservableCollection<string> _logs = new();
    private readonly ObservableCollection<string> _targets = new();
    private readonly DispatcherTimer _pollTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private static readonly Random Rng = new();

    public MainWindow()
    {
        InitializeComponent();
        _cardPlay = new CardPlaySender(_net);
        _midGameJoin = new MidGameJoinSender(_net);
        _trade = new TradeSender(_net);
        _heal = new HealSender(_net);
        _turn = new TurnSender(_net);
        _damage = new DamageSender(_net);
        _status = new StatusEffectSender(_net);
        _mana = new ManaSender(_net);
        _map = new MapSender(_net);
        _event = new EventSender(_net);
        _resurrect = new ResurrectSender(_net);
        LogList.ItemsSource = _logs;
        TargetCombo.ItemsSource = _targets;
        _net.OnLog += OnLog;
        _net.OnConnectedEvent += OnConnected;
        _net.OnDisconnectedEvent += OnDisconnected;
        _net.OnWelcomeEvent += OnWelcome;
        _pollTimer.Tick += (_, _) => _net.PollEvents();
        Closing += (_, _) =>
        {
            _pollTimer.Stop();
            _net.Stop();
        };
    }

    private void OnLog(string level, string msg)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}][{level}] {msg}");
            if (_logs.Count > 500) _logs.RemoveAt(_logs.Count - 1);
        });
    }

    private void ConnectBtn_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _net.SetConnectionKey(KeyBox.Text ?? "");
        _net.PlayerName = string.IsNullOrWhiteSpace(NameBox.Text) ? "AI Bot" : NameBox.Text;
        if (!_net.Start()) return;
        if (int.TryParse(PortBox.Text, out int port))
            _net.ConnectToServer(IpBox.Text ?? "127.0.0.1", port);
        _pollTimer.Start();
    }

    private void DisconnectBtn_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _net.Disconnect();
        _pollTimer.Stop();
    }

    private void OnConnected(string ep)
    {
        Dispatcher.UIThread.Post(() =>
        {
            StatusText.Text = $"已连接 ({ep})";
            ConnectBtn.IsEnabled = false;
            DisconnectBtn.IsEnabled = true;
        });
    }

    private void OnDisconnected()
    {
        Dispatcher.UIThread.Post(() =>
        {
            StatusText.Text = "未连接";
            ConnectBtn.IsEnabled = true;
            DisconnectBtn.IsEnabled = false;
            PlayCardBtn.IsEnabled = false;
            ResolveBtn.IsEnabled = false;
            _targets.Clear();
        });
    }

    private void OnWelcome(WelcomeData data)
    {
        Dispatcher.UIThread.Post(() =>
        {
            StatusText.Text = $"已连接 PlayerId={data.PlayerId}";
            _targets.Clear();
            var players = data.PlayerList ?? data.Players;
            if (players != null)
            {
                foreach (var p in players)
                {
                    if (!string.IsNullOrEmpty(p.PlayerId) && p.PlayerId != data.PlayerId)
                        _targets.Add(p.PlayerId);
                }
            }
            if (_targets.Count > 0 && TargetCombo.SelectedIndex < 0)
                TargetCombo.SelectedIndex = 0;
            bool hasTarget = _targets.Count > 0;
            PlayCardBtn.IsEnabled = hasTarget;
            ResolveBtn.IsEnabled = hasTarget;
            // 设置房主 ID 用于中途加入。
            _midGameJoin.SetHostFromWelcome(data);
            MidGameJoinBtn.IsEnabled = _midGameJoin.CanRequest;
        });
    }

    private void MidGameJoinBtn_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => _midGameJoin.SendJoinRequest();

    private void PlayCardBtn_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TargetCombo.SelectedItem is string target && !string.IsNullOrEmpty(target))
            _cardPlay.SendOnRemoteCardUse(target, target);
    }

    private void ResolveBtn_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TargetCombo.SelectedItem is string target && !string.IsNullOrEmpty(target))
            _cardPlay.SendOnRemoteCardResolved(target);
    }

    // 目标玩家：优先下拉选中，兜底自身。
    private string TargetOrSelf => (TargetCombo.SelectedItem as string) ?? _net.SelfPlayerId;

    private void TradeBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _trade.RunRandomFlow(_net.SelfPlayerId, TargetOrSelf);

    private void BattleHealBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _heal.RunRandom(_net.SelfPlayerId);

    private void GapHealBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _heal.SendGapHeal(TargetOrSelf);

    private void EndTurnBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _turn.SendEndTurnRequest($"battle_{Rng.Next(1, 999)}", Rng.Next(1, 10));

    private void TurnBoundaryBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _turn.RunRandom();

    private void DamageBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _damage.RunRandom(_net.SelfPlayerId);

    private void StatusBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _status.RunRandom(_net.SelfPlayerId);

    private void ManaBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _mana.RunRandom();

    private void MapEnterBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _map.RunRandom();

    private void MapVoteBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _map.SendMapNodeVoteCast(1, Rng.Next(0, 7), Rng.Next(0, 7), 1, Rng.Next(0, 7), Rng.Next(0, 7));

    private void EventSelBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _event.RunRandom();

    private void EventVoteBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _event.SendEventVoteCast($"event_{Rng.Next(1, 20)}", Rng.Next(0, 3));

    private void ResurrectReqBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _resurrect.SendResurrectRequest(TargetOrSelf);

    private void ResurrectedBtn_OnClick(object? s, Avalonia.Interactivity.RoutedEventArgs e)
        => _resurrect.RunRandom();
}