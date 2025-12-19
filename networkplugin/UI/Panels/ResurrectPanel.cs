using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.UI.Panels;
using UnityEngine;
using TMPro;
using LBoL.Presentation.UI;

namespace NetworkPlugin.UI.Panels;

/// <summary>
/// 复活面板类
/// 处理玩家之间的复活功能
/// </summary>
public class ResurrectPanel : UiPanel
{
    public override PanelLayer Layer => PanelLayer.Popup;

    // UI组件
    [SerializeField] private Transform deadPlayersList;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private UnityEngine.UI.Button resurrectButton;
    [SerializeField] private UnityEngine.UI.Button cancelButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI goldAmount;

    // 复活数据
    private List<DeadPlayerEntry> deadPlayers = new List<DeadPlayerEntry>();
    private DeadPlayerEntry selectedPlayer = null;
    private int resurrectionCost = 100;

    public void Awake()
    {
        resurrectButton?.onClick.AddListener(OnResurrectPlayer);

        cancelButton?.onClick.AddListener(OnCancelResurrect);

        statusText?.text = "选择要复活的玩家";
    }

    public IEnumerator ShowResurrectUI()
    {
        // 初始化死亡玩家列表
        InitializeDeadPlayers();

        if (deadPlayers.Count == 0)
        {
            statusText.text = "没有需要复活的玩家";
            yield return new WaitForSeconds(2f);
            Hide();
            yield break;
        }

        // 创建UI条目
        CreatePlayerEntries();

        // 更新金币显示
        UpdateGoldDisplay();

        // 显示面板
        Show();

        // 等待选择
        yield return new WaitUntil(() => selectedPlayer != null || this.IsHidden);

        if (selectedPlayer != null)
        {
            // 确认复活
            yield return ConfirmResurrection();
        }

        Hide();
    }

    private void InitializeDeadPlayers()
    {
        deadPlayers.Clear();

        // 这里需要根据实际的游戏状态获取死亡玩家列表
        // 示例数据，实际实现需要从网络状态或游戏状态获取
        deadPlayers.Add(new DeadPlayerEntry
        {
            PlayerName = "Player2",
            Level = 5,
            DeadCause = "战斗失败",
            ResurrectionCost = CalculateResurrectionCost(5),
            CanResurrect = true
        });

        deadPlayers.Add(new DeadPlayerEntry
        {
            PlayerName = "Player3",
            Level = 3,
            DeadCause = "陷阱伤害",
            ResurrectionCost = CalculateResurrectionCost(3),
            CanResurrect = false // 示例：某些情况不能复活
        });
    }

    private int CalculateResurrectionCost(int playerLevel)
    {
        // 复活成本计算：基础成本 + 等级 * 20
        return 50 + playerLevel * 20;
    }

    private void CreatePlayerEntries()
    {
        // 清除现有条目
        if (deadPlayersList != null)
        {
            foreach (Transform child in deadPlayersList)
            {
                Destroy(child.gameObject);
            }
        }

        // 创建新条目
        for (int i = 0; i < deadPlayers.Count; i++)
        {
            var player = deadPlayers[i];
            var entry = CreatePlayerEntry(player, i);
            entry.transform.SetParent(deadPlayersList, false);
        }
    }

    private GameObject CreatePlayerEntry(DeadPlayerEntry player, int index)
    {
        if (playerEntryPrefab == null)
        {
            // 如果没有预制体，创建一个简单的UI对象
            var entry = new GameObject($"PlayerEntry_{index}");
            entry.AddComponent<RectTransform>();

            // 添加背景
            var bg = entry.AddComponent<UnityEngine.UI.Image>();
            bg.color = index % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f, 0.8f) : new Color(0.3f, 0.3f, 0.3f, 0.8f);

            // 添加按钮组件
            var button = entry.AddComponent<UnityEngine.UI.Button>();
            button.onClick.AddListener(() => OnPlayerSelected(player));

            // 添加文本
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(entry.transform);
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{player.PlayerName} (Lv.{player.Level}) - {player.ResurrectionCost} Gold\n死因: {player.DeadCause}";
            text.color = player.CanResurrect ? Color.white : Color.gray;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Left;

            var rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(10, 2);
            rectTransform.offsetMax = new Vector2(-10, -2);

            return entry;
        }
        else
        {
            // 使用预制体创建
            var entry = Instantiate(playerEntryPrefab);
            SetupPlayerEntry(entry, player);
            return entry;
        }
    }

    private void SetupPlayerEntry(GameObject entry, DeadPlayerEntry player)
    {
        // 设置预制体组件
        var button = entry.GetComponent<UnityEngine.UI.Button>();
        button?.onClick.AddListener(() => OnPlayerSelected(player));

        var text = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = $"{player.PlayerName} (Lv.{player.Level}) - {player.ResurrectionCost} Gold\n死因: {player.DeadCause}";
            text.color = player.CanResurrect ? Color.white : Color.gray;
        }
    }

    private void OnPlayerSelected(DeadPlayerEntry player)
    {
        if (!player.CanResurrect)
        {
            statusText?.text = "该玩家无法复活";
            return;
        }

        selectedPlayer = player;
        resurrectionCost = player.ResurrectionCost;

        // 更新UI显示
        statusText?.text = $"已选择: {player.PlayerName} - 复活成本: {resurrectionCost} Gold";

        costText?.text = $"复活成本: {resurrectionCost} Gold";

        // 检查是否有足够金币
        bool canAfford = base.GameRun.Money >= resurrectionCost;
        resurrectButton?.interactable = canAfford;

        if (!canAfford && statusText != null)
            statusText.text += "\n金币不足！";
    }

    private void UpdateGoldDisplay()
    {
        goldAmount?.text = $"当前金币: {base.GameRun.Money}";
    }

    private void OnResurrectPlayer()
    {
        if (selectedPlayer == null || !selectedPlayer.CanResurrect)
            return;

        if (base.GameRun.Money < resurrectionCost)
        {
            statusText?.text = "金币不足，无法复活！";
            return;
        }

        // 扣除金币
        base.GameRun.ConsumeMoney(resurrectionCost);

        // 执行复活逻辑
        StartCoroutine(ExecuteResurrection());
    }

    private IEnumerator ExecuteResurrection()
    {
        statusText?.text = $"正在复活 {selectedPlayer.PlayerName}...";

        // 扣除金币动画
        yield return new WaitForSeconds(1f);

        // 这里执行实际的复活逻辑
        // 需要与网络同步，通知其他玩家复活事件
        yield return ResurrectPlayer(selectedPlayer);

        statusText?.text = $"{selectedPlayer.PlayerName} 已复活！";

        // 发送网络事件
        SendResurrectionEvent(selectedPlayer);

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator ResurrectPlayer(DeadPlayerEntry player)
    {
        // 实际的复活逻辑
        // 这里需要根据游戏的具体实现来处理
        // 示例：恢复玩家的生命值，重新加入游戏等

        // 如果是本地玩家，直接恢复
        if (player.PlayerName == base.GameRun.Player.Name)
        {
            base.GameRun.Heal(base.GameRun.Player.MaxHp, false, null);
            base.GameRun.Player.Status = UnitStatus.Alive;
        }
        else
        {
            // 其他玩家需要通过网络同步
            // 发送复活请求到服务器或其他玩家
        }

        yield return null;
    }

    private void SendResurrectionEvent(DeadPlayerEntry player)
    {
        try
        {
            // 发送复活事件到网络
            var syncManager = Core.ModService.ServiceProvider.GetService<NetworkPlugin.Core.ISynchronizationManager>();
            if (syncManager != null)
            {
                var resurrectionData = new Dictionary<string, object>
                {
                    ["EventType"] = "PlayerResurrected",
                    ["PlayerName"] = player.PlayerName,
                    ["Cost"] = resurrectionCost,
                    ["Timestamp"] = DateTime.Now.Ticks
                };

                // 这里需要根据实际的网络管理器实现发送逻辑
                // syncManager.SendGameEvent(resurrectionData);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ResurrectPanel] 发送复活事件失败: {ex.Message}");
        }
    }

    private void OnCancelResurrect()
    {
        selectedPlayer = null;
        Hide();
    }
}
