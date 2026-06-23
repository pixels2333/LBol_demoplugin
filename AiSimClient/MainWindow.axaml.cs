using System;
using System.Collections.Generic;
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
    private readonly ObservableCollection<TargetEntry> _targets = new();
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
        _net.OnBattleStartEvent += OnBattleStart;
        _net.OnEnemyDiscoveredEvent += OnEnemyDiscovered;
        _net.OnEnemyStateChangedEvent += OnEnemyStateChanged;
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
        // 角色ID用于接收端创建真实施法者；留空时接收端兜底用本地玩家角色。
        _net.CharacterId = string.IsNullOrWhiteSpace(CharIdBox.Text) ? "" : CharIdBox.Text;
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
            // 仅重建玩家条目，保留已收到的敌人条目。
            RemoveTargets(isEnemy: false);
            var players = data.PlayerList ?? data.Players;
            if (players != null)
            {
                foreach (var p in players)
                {
                    if (!string.IsNullOrEmpty(p.PlayerId) && p.PlayerId != data.PlayerId)
                        _targets.Add(new TargetEntry
                        {
                            Id = p.PlayerId,
                            Name = p.PlayerName,
                            Display = string.IsNullOrWhiteSpace(p.PlayerName)
                                ? $"玩家:{p.PlayerId}"
                                : $"玩家:{p.PlayerName}",
                            IsEnemy = false,
                        });
                }
            }
            if (_targets.Count > 0 && TargetCombo.SelectedIndex < 0)
                TargetCombo.SelectedIndex = 0;
            UpdateTargetButtons();
            // 设置房主 ID 用于中途加入。
            _midGameJoin.SetHostFromWelcome(data);
            MidGameJoinBtn.IsEnabled = _midGameJoin.CanRequest;
        });
    }

    // 收到 OnBattleStart：更新敌人条目（替换旧的敌人，保留玩家条目）。
    private void OnBattleStart(List<EnemyInfo> enemies)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RemoveTargets(isEnemy: true);
            foreach (var e in enemies)
            {
                _targets.Add(new TargetEntry
                {
                    Id = e.EnemyId,
                    Name = e.EnemyName,
                    Display = string.IsNullOrWhiteSpace(e.EnemyName)
                        ? $"敌人:{e.EnemyId}"
                        : $"敌人:{e.EnemyName}",
                    IsEnemy = true,
                });
            }
            if (_targets.Count > 0 && TargetCombo.SelectedIndex < 0)
                TargetCombo.SelectedIndex = 0;
            UpdateTargetButtons();
        });
    }

    // 收到单个敌人发现事件（来自 BattleEnemyIntentChanged）：去重追加到下拉。
    private void OnEnemyDiscovered(EnemyInfo enemy)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // 去重：已存在同 Id 的敌人条目则不重复添加。
            for (int i = 0; i < _targets.Count; i++)
            {
                if (_targets[i].IsEnemy && string.Equals(_targets[i].Id, enemy.EnemyId, StringComparison.Ordinal))
                {
                    return;
                }
            }

            _targets.Add(new TargetEntry
            {
                Id = enemy.EnemyId,
                Name = enemy.EnemyName,
                Display = string.IsNullOrWhiteSpace(enemy.EnemyName)
                    ? $"敌人:{enemy.EnemyId}"
                    : $"敌人:{enemy.EnemyName}",
                IsEnemy = true,
            });
            if (_targets.Count > 0 && TargetCombo.SelectedIndex < 0)
                TargetCombo.SelectedIndex = 0;
            UpdateTargetButtons();
        });
    }

    // 收到敌人状态变化事件（来自 BattleEnemyStateChanged）：更新下拉列表中敌人的显示名（含HP），
    // 死亡时移除条目。
    private void OnEnemyStateChanged(EnemyStateData enemy)
    {
        Dispatcher.UIThread.Post(() =>
        {
            string id = !string.IsNullOrWhiteSpace(enemy.Id) ? enemy.Id! : enemy.SpawnId ?? "";
            for (int i = _targets.Count - 1; i >= 0; i--)
            {
                if (!_targets[i].IsEnemy) continue;
                if (!string.Equals(_targets[i].Id, id, StringComparison.Ordinal)) continue;

                if (!enemy.IsAlive || enemy.IsDying || enemy.CurrentHp <= 0)
                {
                    _targets.RemoveAt(i);
                    OnLog("INFO", $"敌人 {enemy.Name ?? id} 已死亡，从列表移除");
                }
                else
                {
    _targets[i].Display = $"敌人:{enemy.Name} HP:{enemy.CurrentHp}/{enemy.MaxHp}";
                }
                return;
            }
        });
    }

    private void RemoveTargets(bool isEnemy)
    {
        for (int i = _targets.Count - 1; i >= 0; i--)
        {
            if (_targets[i].IsEnemy == isEnemy)
                _targets.RemoveAt(i);
        }
    }

    private void UpdateTargetButtons()
    {
        bool hasTarget = _targets.Count > 0;
        PlayCardBtn.IsEnabled = hasTarget;
        ResolveBtn.IsEnabled = hasTarget;
    }

    private void MidGameJoinBtn_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => _midGameJoin.SendJoinRequest();

    private void PlayCardBtn_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TargetCombo.SelectedItem is TargetEntry target && !string.IsNullOrEmpty(target.Id))
            _cardPlay.SendOnRemoteCardUse(target.Id, target.Name, target.IsEnemy,
                CardIdBox.Text ?? "", CardNameBox.Text ?? "", CardTypeBox.Text ?? "",
                GunNameBox.Text ?? "", GunTypeBox.Text ?? "");
    }

    private void ResolveBtn_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (TargetCombo.SelectedItem is TargetEntry target && !string.IsNullOrEmpty(target.Id))
            _cardPlay.SendOnRemoteCardResolved(target.Id);
    }

    // 目标玩家：优先下拉选中，兜底自身。
    private string TargetOrSelf => (TargetCombo.SelectedItem as TargetEntry)?.Id ?? _net.SelfPlayerId;

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