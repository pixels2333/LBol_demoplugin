using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.Panels;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.RoomSync;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Snapshot;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Network.MidGameJoin;

/// <summary>
/// 客户端追赶执行器（最小骨架）：
/// - 接收主机 FullStateSnapshot（重点是 MapState）并暂存。
/// - 当本地 GameRun 可用（通常在地图界面）时，将 MapState 尽力应用到当前地图。
///
/// 备注：本类不负责“创建新 GameRun”；无本地 GameRun 时仅暂存并等待。
/// </summary>
public sealed class MapCatchUpOrchestrator
{
    #region 字段与常量
    /// <summary>日志记录器，用于输出追赶过程中的诊断与警告信息。</summary>
    private readonly ManualLogSource _logger;
    /// <summary>房间同步管理器，负责在地图状态应用完成后请求主机房间状态。</summary>
    private readonly RoomSyncManager _roomSyncManager;

    /// <summary>线程锁，保护 _pendingSnapshot、_applied、_session 等共享状态，防止 PumpMainThread 与 SetPendingFullSnapshot 并发读写。</summary>
    private readonly object _lock = new();

    /// <summary>主机下发的最新完整状态快照；在应用完成前暂存于此，供 PumpMainThread 周期性读取。</summary>
    private FullStateSnapshot _pendingSnapshot;
    /// <summary>_pendingSnapshot 被接收时的 UTC 时间戳（Ticks），用于日志记录和调试追踪。</summary>
    private long _pendingReceivedAtUtcTicks;
    /// <summary>_pendingSnapshot 对应的检查点标识；用于种子不匹配日志去重和房间状态请求去重。</summary>
    private string _pendingCheckpointId = string.Empty;
    /// <summary>上次记录种子不匹配警告日志的 UTC 时间戳（Ticks），配合 _lastSeedMismatchCheckpointId 实现 5 秒节流。</summary>
    private long _lastSeedMismatchLogAtUtcTicks;
    /// <summary>上次记录种子不匹配警告日志的检查点标识；与 _lastSeedMismatchLogAtUtcTicks 共同实现日志去重。</summary>
    private string _lastSeedMismatchCheckpointId = string.Empty;
    /// <summary>标记当前暂存快照是否已完整应用；为 true 时 PumpMainThread 会触发清理。</summary>
    private bool _applied;

    /// <summary>当前活跃的追赶会话，绑定到特定的 _pendingSnapshot；会话完成后会被清空。</summary>
    private CatchUpSession _session;

    /// <summary>标记是否已为加入者弹出"开始游戏"提示对话框；防止重复 spam。</summary>
    private bool _startGamePrompted;

    /// <summary>上次请求房间状态时使用的检查点标识；用于防止对同一检查点重复请求房间状态。</summary>
    private string _lastRoomStateRequestedCheckpointId = string.Empty;
    #endregion

    #region 嵌套类型
    /// <summary>
    /// 追赶会话，记录地图状态追赶进度
    /// </summary>
    private sealed class CatchUpSession
    {
        /// <summary>绑定的主机完整状态快照引用；所有追赶数据均来源于此快照。</summary>
        public FullStateSnapshot Snapshot;
        /// <summary>本次追赶对应的检查点标识；用于日志去重和房间状态请求去重。</summary>
        public string CheckpointId;
        /// <summary>快照被接收时的 UTC 时间戳（Ticks）；用于调试和超时检测。</summary>
        public long ReceivedAtUtcTicks;
        /// <summary>追赶会话创建时的 UTC 时间戳（Ticks）；用于调试和会话生命周期追踪。</summary>
        public long CreatedAtUtcTicks;

        /// <summary>标记路径重建是否已初始化（首次进入 StepApplyPathByEnterNode 时设为 true）。</summary>
        public bool PathInitialized;
        /// <summary>路径历史已处理到的节点索引；与 PathHistory.Count 比较判断是否完成路径重建。</summary>
        public int PathIndex;

        /// <summary>主机下发的节点状态键值对列表（Key=节点唯一键，Value=状态字符串）；由 NodeStates 字典转换而来。</summary>
        public List<KeyValuePair<string, string>> NodeStatePairs;
        /// <summary>节点状态已处理到的列表索引；与 NodeStatePairs.Count 比较判断是否完成状态同步。</summary>
        public int NodeStateIndex;

        /// <summary>标记主机当前位置（VisitingNode）是否已同步到本地地图。</summary>
        public bool CurrentLocationApplied;
        /// <summary>标记地图状态应用完成后是否已向主机请求过房间状态。</summary>
        public bool RoomStateRequested;
        /// <summary>标记本次追赶会话是否已完全完成（路径、节点状态、当前位置、房间状态均就绪）。</summary>
        public bool Completed;

        /// <summary>主机下发的已通关节点键集合；用于结算时判断节点是否已被主机视为通过。</summary>
        public HashSet<string> ClearedNodeKeys;
        /// <summary>本地已执行结算的节点键集合；防止同一节点重复结算。</summary>
        public HashSet<string> SettledNodeKeys;

        /// <summary>当前等待结算的节点唯一键；非空时 PumpMainThread 会暂停路径/状态推进，驱动结算 UI。</summary>
        public string PendingSettlementNodeKey;
        /// <summary>当前等待结算的站点实例（BossStation 或 BattleStation）；结算 UI 驱动逻辑依赖此字段判断房间类型。</summary>
        public Station PendingSettlementStation;
        /// <summary>标记当前挂起结算的 Boss 遗物弹窗是否已在本地展示过；防止重复弹窗。</summary>
        public bool PendingBossExhibitShown;
        /// <summary>标记当前挂起结算的奖励面板是否已在本地展示过；防止重复弹窗。</summary>
        public bool PendingRewardShown;
    }

    #endregion

    #region 构造函数
    public MapCatchUpOrchestrator(ManualLogSource logger, RoomSyncManager roomSyncManager)
    {
        _logger = logger ?? Plugin.Logger;
        _roomSyncManager = roomSyncManager ?? throw new ArgumentNullException(nameof(roomSyncManager));
    }
    #endregion

    #region 公共方法 — 快照管理与主线程驱动
    /// <summary>
    /// 主线程驱动入口：
    /// - 当本地 GameRun 就绪时，尝试应用待处理的快照。
    /// - 若加入者尚未开始本地对局，尽力弹出开始游戏提示。
    /// </summary>
    /// <remarks>
    /// 由 <see cref="Plugin.Update"/> 周期性调用；因此使用较大的步进预算以保持追赶进度。
    /// </remarks>
    public void PumpMainThread()
    {
        try // 主线程循环中的异常不应导致整个插件崩溃，静默捕获并依赖下次 tick 重试
        {
            // 优先尝试应用地图状态；PumpMainThread 本身有节流（Plugin.Update），
            // 所以这里使用较大的步进预算，保证每 tick 能推进足够进度。
            // 增量式应用快照：每 tick 推进 4 个路径节点 + 120 个节点状态，避免单帧卡顿导致 UI 掉帧
            TryApplyPendingToCurrentRun(pathStepsBudget: 4, nodeStatesBudget: 120);

            // 若仍有待处理快照且加入者尚未开局，尝试弹出 StartGamePanel 提示。
            // 先检查是否还有待处理快照且尚未应用；避免单机局也弹窗干扰
            if (TryGetPendingFullSnapshot(out FullStateSnapshot snapshot))
            {
                // 仅在有快照时尝试提示，避免无关场景（如单机局）也弹出提示干扰用户体验
                TryPromptStartGameForJoiner_NoThrow(snapshot);
            }
        }
        catch // 主线程循环中的异常不应导致整个插件崩溃，静默捕获并依赖下次 tick 重试
        {
            // TODO: 应记录 PumpMainThread 异常，便于排查主线程驱动中断原因
            // ignored
        }
    }

    /// <summary>
    /// 设置待应用的完整状态快照（通常来自中途加入时的 FullStateSyncResponse）。
    /// </summary>
    /// <remarks>
    /// UI 相关弹窗必须在主线程执行；周期性的 PumpMainThread 会负责处理这些提示。
    /// </remarks>
    public void SetPendingFullSnapshot(FullStateSnapshot snapshot)
    {
        if (snapshot == null) // 防御性校验：服务器可能下发空快照，直接丢弃避免后续 NullReferenceException
        {
            return; // 空快照无需处理，直接返回
        }

        lock (_lock) // 保护共享状态：pendingSnapshot、session 等字段可能被 PumpMainThread 并发读取
        {
            _pendingSnapshot = snapshot; // 暂存主机下发的完整状态快照，供后续增量应用
            _pendingReceivedAtUtcTicks = DateTime.UtcNow.Ticks; // 记录接收时间，用于日志追踪和超时判断
            _pendingCheckpointId = snapshot.MapState?.LastCheckpointId ?? string.Empty; // 提取检查点 ID，用于房间状态请求去重和日志标识
            _applied = false; // 新快照到达后重置应用标记，允许重新进入追赶流程
            _startGamePrompted = false; // 新快照到达后重置提示标记，允许再次弹窗提示加入者开局
            _session = null; // 丢弃旧会话，下次 PumpMainThread 会创建与新快照绑定的新会话

            // 允许为新快照再次请求房间状态，避免旧检查点造成重复请求阻断。
            _lastRoomStateRequestedCheckpointId = string.Empty; // 清空上次请求的检查点 ID，使新快照能触发新的房间状态请求
        }

        _logger?.LogInfo($"[MapCatchUp] Pending snapshot stored: ts={snapshot.Timestamp}, checkpoint={_pendingCheckpointId}"); // 记录快照接收日志，便于排查加入者是否收到主机状态

        // UI 弹窗需在主线程执行，由 PumpMainThread 周期性处理。
    }

    /// <summary>
    /// 尝试获取尚未应用的待处理完整快照。
    /// 用于加入者启动游戏锁从主机快照中读取配置。
    /// 当无待处理快照或已被应用时返回 false。
    /// </summary>
    /// <param name="snapshot">输出参数：获取到的待处理快照。</param>
    /// <returns>true 表示获取到未应用的待处理快照；false 表示无快照或已应用。</returns>
    public bool TryGetPendingFullSnapshot(out FullStateSnapshot snapshot)
    {
        snapshot = null; // 默认输出为 null

        lock (_lock) // 与 PumpMainThread 并发读互斥
        {
            if (_pendingSnapshot == null || _applied) // 无待处理快照或已标记为应用完毕
            {
                return false; // 无可用的待处理快照
            }

            snapshot = _pendingSnapshot; // 在锁内复制引用，确保线程安全
            return snapshot != null; // 返回是否成功获取到有效快照
        }
    }

    /// <summary>
    /// 为加入者弹出"开始游戏"提示对话框。
    /// 当检测到主机有进行中的对局且本地无活跃 GameRun 时触发。
    /// 难度、地图种子、关卡列表将锁定为房主设置，仅角色可由加入者自选。
    /// </summary>
    /// <param name="snapshot">主机完整状态快照，包含 GameState 启动配置。</param>
    private void TryPromptStartGameForJoiner_NoThrow(FullStateSnapshot snapshot)
    {
        try
        {
            if (_startGamePrompted) // 已经弹过提示，避免重复 spam
            {
                return; // 已提示过，直接退出
            }

            if (snapshot?.GameState == null) // 快照中未包含游戏状态信息
            {
                return; // 无有效 GameState，无法构建启动配置
            }

            // 仅在本地无活跃对局时提示；若已有 GameRun 则不应打断。
            if (GameStateUtils.GetCurrentGameRun() != null) // 本地已有正在运行的游戏局
            {
                return; // 本地已有对局，跳过提示
            }

            // UI 尚未初始化时无法弹窗，需等待下一帧重试。
            if (!UiManager.IsInitialized) // UI 管理器未就绪（如还在加载场景）
            {
                return; // UI 未就绪，延迟到下次 PumpMainThread 再试
            }

            // 必须存在主机的启动配置；否则无法保证确定性对齐。
            if (snapshot.GameState.RootSeed == null || // 主机 RootSeed 缺失（地图种子无法对齐）
                snapshot.GameState.Difficulty == null || // 主机难度缺失
                snapshot.GameState.StageTypeNames == null || // 主机关卡列表缺失
                snapshot.GameState.StageTypeNames.Count == 0) // 主机关卡列表为空
            {
                return; // 启动配置不完整，无法安全提示开始游戏
            }

            _startGamePrompted = true; // 标记已提示，防止重复弹窗

            // 构建与主机关卡列表一致的 StartGameData。
            StartGameData data = new StartGameData
            {
                StagesCreateFunc = () =>
                {
                    List<Stage> stages = new(); // 临时容器：按主机顺序创建的关卡列表
                    foreach (string name in snapshot.GameState.StageTypeNames) // 遍历主机快照中的关卡类型名
                    {
                        if (string.IsNullOrWhiteSpace(name)) // 关卡名称为空或空白（协议异常）
                        {
                            continue; // 跳过无效关卡名称
                        }

                        Stage s = Library.CreateStage(name); // 通过游戏库按名称创建关卡实例
                        if (s != null) // 关卡创建成功
                        {
                            stages.Add(s); // 加入关卡列表
                        }
                    }

                    // 若关卡创建全部失败，回退到游戏默认模式（竹林、玄武涧、风神湖、终幕）。
                    if (stages.Count == 0) // 所有主机关卡均创建失败（缺少 Mod 或类型名错误）
                    {
                        return new Stage[]
                        {
                            Library.CreateStage<BambooForest>(), // 第一幕：竹林
                            Library.CreateStage<XuanwuRavine>(), // 第二幕：玄武涧
                            Library.CreateStage<WindGodLake>().AsNormalFinal(), // 第三幕：风神湖（普通结局）
                            Library.CreateStage<FinalStage>().AsTrueEndFinal(), // 第四幕：终幕（真结局）
                        };
                    }

                    // 对最后两个关卡应用终幕标志，使其行为与默认模式一致。
                    if (stages.Count >= 4) // 关卡数不少于 4 个时，按默认模式设置终幕标记
                    {
                        stages[2]?.AsNormalFinal(); // 第三幕标记为普通结局
                        stages[3]?.AsTrueEndFinal(); // 第四幕标记为真结局
                    }

                    return stages.ToArray(); // 将列表转为数组返回给 StartGamePanel
                },
                DebutAdventure = typeof(Debut), // 设置初始冒险类型为 Debut（新手教程）
            };

            // 友好提示：角色可由玩家自选，但难度/种子/关卡列表锁定为房主设置。
            try
            {
                string diff = snapshot.GameState.Difficulty?.ToString() ?? "<unknown>"; // 提取主机难度文本用于弹窗提示
                UiManager.GetDialog<MessageDialog>().Show(new MessageContent
                {
                    Text = "检测到正在进行的联机对局。\n\n请先选择角色开始新局以加入追赶。\n\n提示：难度/地图种子/关卡列表将自动锁定为房主设置。\n（你仍可以选择角色）", // 弹窗提示文本：告知玩家配置锁定情况
                    Icon = MessageIcon.Warning, // 使用警告图标引起玩家注意
                    Buttons = DialogButtons.Confirm, // 仅显示确认按钮
                    OnConfirm = () => // 玩家点击确认后的回调
                    {
                        try
                        {
                            UiManager.GetPanel<StartGamePanel>()?.Show(data); // 打开开始游戏面板并传入预构建的 StartGameData
                        }
                        catch
                        {
                            // TODO: 应记录异常信息，避免静默吞掉错误
                            // ignored
                        }
                    },
                });
            }
            catch
            {
                // 若弹窗失败，仍尝试直接打开开始游戏面板。
                UiManager.GetPanel<StartGamePanel>()?.Show(data); // 兜底：直接显示开始游戏面板
            }
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }

    /// <summary>
    /// 若存在待应用快照且本地 GameRun 已就绪，则尽力将 MapState 应用到当前地图。
    /// </summary>
    /// <param name="pathStepsBudget">每 tick 最多重建的路径节点数</param>
    /// <param name="nodeStatesBudget">每 tick 最多应用的节点状态数</param>
    /// <returns>应用成功返回 true；未完成或条件不满足返回 false</returns>
    public bool TryApplyPendingToCurrentRun(int pathStepsBudget = 1, int nodeStatesBudget = 25)
    {
        FullStateSnapshot snapshot; // 从锁外声明，避免在锁内分配大对象
        long receivedAt; // 快照接收时间，用于日志和调试追踪
        string checkpointId; // 检查点 ID，用于种子不匹配日志和房间状态请求去重

        lock (_lock) // 保护共享状态：与 SetPendingFullSnapshot 并发写互斥
        {
            if (_pendingSnapshot == null || _applied) // 无待处理快照或已应用完毕，本轮无事可做
            {
                return false;
            }

            snapshot = _pendingSnapshot; // 在锁内读取引用，确保拿到一致的快照对象
            receivedAt = _pendingReceivedAtUtcTicks; // 记录接收时间，供后续日志输出
            checkpointId = _pendingCheckpointId; // 提取检查点 ID，用于日志和房间状态请求
        }

        try
        {
            GameRunController run = GameStateUtils.GetCurrentGameRun(); // 获取当前正在运行的 GameRun；若玩家尚未开局则返回 null
            if (run == null || run.CurrentMap == null) // 本地 GameRun 或地图尚未就绪，暂不能应用快照
            {
                return false;
            }

            if (snapshot.MapState == null) // 快照中不含地图状态（协议异常或旧版本），无法追赶
            {
                return false;
            }

            if (!IsLikelySameMapSeed(run, snapshot.MapState)) // 地图种子不一致说明不是同一局（如房主已切换关卡）
            {
                MaybeLogSeedMismatch(checkpointId); // 记录种子不匹配警告（带 5 秒节流）
                return false; // 等待玩家重新开始新局以匹配房主种子
            }

            // 确保会话存在（并与当前待处理快照绑定）。
            CatchUpSession session = GetOrCreateSession_NoThrow(snapshot, checkpointId, receivedAt); // 获取或创建与新快照绑定的追赶会话；若快照已过期则返回 null
            if (session == null)
            {
                return false;
            }

            // 0) 优先驱动结算 UI：弹窗打开时会暂停追赶，等待用户关闭后再继续。
            if (StepDriveSettlementUi_NoThrow(run, session)) // 若有待结算节点且面板未关闭，返回 true 暂停后续步骤
            {
                return false; // 本轮暂停，等待用户操作后再继续推进
            }

            // 预算值 sanitize：负预算视为零，防止非法参数导致越界或死循环
            if (pathStepsBudget < 0) // 防御性校验：调用方可能传入负数
            {
                pathStepsBudget = 0; // 负数重置为零，避免循环条件异常
            }

            if (nodeStatesBudget < 0) // 防御性校验：调用方可能传入负数
            {
                nodeStatesBudget = 0; // 负数重置为零，避免数组越界
            }

            // 0) 路径重建：通过 GameMap.EnterNode(forced=true) 逐步重建内部路径，
            // RoomStateSyncPatch 会忽略 forced EnterNode，因此不会触发多余的 RoomStateRequest。
            StepApplyPathByEnterNode(run.CurrentMap, snapshot.MapState.PathHistory, session, pathStepsBudget); // 增量推进路径历史；每 tick 推进 budget 个节点，避免单帧卡顿

            // 路径推进后，刚清理的战斗节点可能需要弹出奖励/结算 UI。
            if (StepDriveSettlementUi_NoThrow(run, session)) // 路径推进后可能触发新的结算节点，再次检查 UI 状态
            {
                return false; // 结算 UI 未关闭，暂停本轮后续步骤
            }

            // 1) 节点状态：在小批量模式下应用房主的 node.Status 字符串。
            if (IsPathDone(session)) // 仅当路径完全重建后才开始应用节点状态，避免顺序错乱
            {
                StepApplyNodeStates(run.CurrentMap, session, nodeStatesBudget); // 批量应用节点状态；每 tick 推进 budget 个，保持帧率稳定
            }

            // 2) 当前位置：仅在路径和节点状态都完成后，设置 VisitingNode。
            if (IsPathDone(session) && IsNodeStatesDone(session) && !session.CurrentLocationApplied) // 确保路径和状态都已完成，再设置当前位置
            {
                TryApplyCurrentLocation(run.CurrentMap, snapshot.MapState); // 将本地 VisitingNode 对齐到房主当前所在节点
                session.CurrentLocationApplied = true; // 标记位置已应用，避免重复设置
            }

            // 3) 房间状态请求：位置对齐后，每个检查点只请求一次房间状态。
            if (IsPathDone(session) && IsNodeStatesDone(session) && session.CurrentLocationApplied && !session.RoomStateRequested) // 所有前置步骤完成后才请求房间状态
            {
                // 追赶设计上是避免 EnterNode 的，因此 RoomStateSyncPatch 不会自动触发，需要手动请求。
                TryRequestRoomStateAfterMapApplied_NoThrow(snapshot); // 请求当前房间状态，确保战斗/事件同步
                session.RoomStateRequested = true; // 标记已请求，避免重复发送
            }

            // 4) 完成判定：仅当所有步骤都完成后才标记为已应用。
            if (IsPathDone(session) && IsNodeStatesDone(session) && session.CurrentLocationApplied && session.RoomStateRequested)
            {
                session.Completed = true;
                MarkApplied();
                ClearPendingAfterApplied_NoThrow();
                _logger?.LogInfo($"[MapCatchUp] Applied MapState (incremental): checkpoint={checkpointId}, receivedAt={receivedAt}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"[MapCatchUp] Apply degraded: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region 私有方法 — 增量追赶核心逻辑

    /// <summary>
    /// 获取或创建与当前待处理快照绑定的追赶会话。
    /// 在锁内检查快照一致性：若外部捕获后又有新快照到达，则放弃本次会话创建，等待下一 tick 处理最新快照。
    /// </summary>
    /// <param name="snapshot">从锁外捕获的快照引用。</param>
    /// <param name="checkpointId">快照对应的检查点标识。</param>
    /// <param name="receivedAtUtcTicks">快照接收时的 UTC 时间戳。</param>
    /// <returns>有效的 CatchUpSession；若快照已过期或已应用则返回 null。</returns>
    private CatchUpSession GetOrCreateSession_NoThrow(FullStateSnapshot snapshot, string checkpointId, long receivedAtUtcTicks)
    {
        try
        {
            lock (_lock) // 与 PumpMainThread 和 SetPendingFullSnapshot 并发互斥
            {
                if (_pendingSnapshot == null || _applied) // 快照已被应用或在锁外期间被清空
                {
                    return null; // 无有效快照，无法创建会话
                }

                // 若在外部捕获与本次调用之间又有新快照到达，则放弃本次 tick。
                // 下一 tick 会捕获并应用最新的待处理快照。
                if (!ReferenceEquals(_pendingSnapshot, snapshot)) // 引用不一致：说明锁外期间有新快照写入
                {
                    return null; // 快照已过期，等待下次 PumpMainThread 处理新快照
                }

                if (_session != null && ReferenceEquals(_session.Snapshot, snapshot) && // 已有会话且绑定同一快照对象
                    string.Equals(_session.CheckpointId, checkpointId ?? string.Empty, StringComparison.Ordinal)) // 检查点标识一致
                {
                    return _session; // 复用现有会话，避免重复创建和状态重置
                }

                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>(); // 节点状态对临时容器
                try
                {
                    if (snapshot?.MapState?.NodeStates != null && snapshot.MapState.NodeStates.Count > 0) // 快照包含节点状态字典
                    {
                        pairs = snapshot.MapState.NodeStates.ToList(); // 将字典转为列表以便按索引增量处理
                    }
                }
                catch
                {
                    pairs = new List<KeyValuePair<string, string>>(); // 异常时回退为空列表，防止 null 引用
                }

                _session = new CatchUpSession // 创建新的追赶会话并绑定到当前快照
                {
                    Snapshot = snapshot, // 绑定完整状态快照引用
                    CheckpointId = checkpointId ?? string.Empty, // 记录检查点标识（回退空字符串）
                    ReceivedAtUtcTicks = receivedAtUtcTicks, // 记录快照接收时间
                    CreatedAtUtcTicks = DateTime.UtcNow.Ticks, // 记录会话创建时间，用于调试和超时检测
                    PathInitialized = false, // 路径尚未初始化，首次 StepApplyPathByEnterNode 时会清空并重建
                    PathIndex = 0, // 路径处理索引从 0 开始
                    NodeStatePairs = pairs, // 绑定节点状态对列表
                    NodeStateIndex = 0, // 节点状态处理索引从 0 开始
                    CurrentLocationApplied = false, // 当前位置尚未同步
                    RoomStateRequested = false, // 房间状态尚未请求
                    Completed = false, // 会话尚未完成

                    ClearedNodeKeys = new HashSet<string>(StringComparer.Ordinal), // 已通关节点键集合（去重）
                    SettledNodeKeys = new HashSet<string>(StringComparer.Ordinal), // 已结算节点键集合（去重）
                    PendingSettlementNodeKey = string.Empty, // 当前等待结算的节点键（空表示无挂起结算）
                    PendingSettlementStation = null, // 当前等待结算的站点实例
                    PendingBossExhibitShown = false, // Boss 遗物展示标记（防止重复弹窗）
                    PendingRewardShown = false, // 奖励面板展示标记（防止重复弹窗）
                };

                try
                {
                    if (snapshot?.MapState?.ClearedNodes != null) // 快照包含已通关节点列表
                    {
                        foreach (string k in snapshot.MapState.ClearedNodes) // 遍历主机下发的已通关节点键
                        {
                            if (!string.IsNullOrWhiteSpace(k)) // 过滤空或空白键
                            {
                                _session.ClearedNodeKeys.Add(k); // 将已通关节点键加入集合，供后续结算跳过判断
                            }
                        }
                    }
                }
                catch
                {
                    // TODO: 应记录异常信息，避免静默吞掉错误
                    // ignored
                }

                return _session; // 返回新创建的追赶会话
            }
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            return null; // 异常时保守返回 null，调用方会跳过本次 tick
        }
    }

    /// <summary>
    /// 判断路径重建是否已完成。
    /// 当 session 为 null 或 PathIndex 已覆盖所有路径历史节点时返回 true。
    /// </summary>
    /// <param name="session">追赶会话，包含 PathIndex 与路径历史总数。</param>
    /// <returns>true 表示路径重建已完成，false 表示仍有节点待处理。</returns>
    private static bool IsPathDone(CatchUpSession session)
    {
        try
        {
            if (session == null) // 防御性校验：session 为 null 时无法判断进度，视为已完成
            {
                return true; // 无有效会话，直接视为路径已完成
            }

            int total = session.Snapshot?.MapState?.PathHistory?.Count ?? 0; // 从快照中提取主机路径历史节点总数；若链路上任一对象为 null 则回退为 0
            return session.PathIndex >= total; // 已处理的节点数大于等于总数时，路径重建完成
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            return true; // 异常时保守返回 true，防止死循环
        }
    }

    /// <summary>
    /// 判断节点状态同步是否已完成。
    /// 当 session 为 null 或 NodeStateIndex 已覆盖所有节点状态对时返回 true。
    /// </summary>
    /// <param name="session">追赶会话，包含 NodeStateIndex 与节点状态对总数。</param>
    /// <returns>true 表示节点状态同步已完成，false 表示仍有节点状态待处理。</returns>
    private static bool IsNodeStatesDone(CatchUpSession session)
    {
        try
        {
            if (session == null) // 防御性校验：session 为 null 时无法判断进度，视为已完成
            {
                return true; // 无有效会话，直接视为节点状态已完成
            }

            int total = session.NodeStatePairs?.Count ?? 0; // 从会话中提取节点状态对总数；若对象为 null 则回退为 0
            return session.NodeStateIndex >= total; // 已处理的节点状态数大于等于总数时，状态同步完成
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            return true; // 异常时保守返回 true，防止死循环
        }
    }

    /// <summary>
    /// 通过强制 EnterNode 逐步重建主机路径历史。
    /// 首次调用时会清空现有路径并重置 VisitingNode。
    /// </summary>
    /// <param name="map">当前游戏地图。</param>
    /// <param name="pathHistory">主机路径历史列表。</param>
    /// <param name="session">追赶会话，跟踪路径索引与初始化状态。</param>
    /// <param name="pathStepsBudget">本次调用允许推进的最大节点数。</param>
    private static void StepApplyPathByEnterNode(GameMap map, List<LocationSnapshot> pathHistory, CatchUpSession session, int pathStepsBudget)
    {
        try
        {
            if (map == null || session == null) // 防御性校验：参数为 null 时无法执行路径重建
            {
                return; // 参数无效，直接退出
            }

            if (!session.PathInitialized) // 首次进入本方法时路径尚未初始化
            {
                // 清除现有路径，避免重复追加导致路径错乱。
                try
                {
                    var list = Traverse.Create(map).Field("_path").GetValue<List<MapNode>>(); // 通过反射获取地图内部 _path 列表
                    list?.Clear(); // 清空原有路径，防止追加产生重复节点
                }
                catch
                {
                    // TODO: 应记录异常信息，避免静默吞掉错误
                    // ignored
                }

                // 重置 VisitingNode，防止首个 EnterNode 误将无关节点标记为已访问。
                try
                {
                    Traverse.Create(map).Property("VisitingNode").SetValue(null); // 通过反射重置 VisitingNode 为 null
                }
                catch
                {
                    // TODO: 应记录异常信息，避免静默吞掉错误
                    // ignored
                }

                session.PathInitialized = true; // 标记路径已初始化，下次调用不再重复清空
                session.PathIndex = 0; // 重置路径索引，从头开始重建
            }

            int total = pathHistory?.Count ?? 0; // 主机路径历史总节点数；pathHistory 为 null 时回退到 0
            if (total <= 0) // 主机路径为空（尚未移动过或协议数据缺失）
            {
                session.PathIndex = 0; // 重置索引，避免后续越界
                return; // 无路径可重建，直接退出
            }

            if (pathStepsBudget <= 0) // 防御性校验：调用方可能传入零或负数
            {
                return; // 无预算可用，本轮不推进
            }

            for (int i = 0; i < pathStepsBudget && session.PathIndex < total; i++) // 增量推进：每 tick 最多推进 budget 个节点
            {
                LocationSnapshot loc = null; // 当前要处理的路径节点快照；异常时回退为 null
                try
                {
                    loc = pathHistory[session.PathIndex]; // 按索引从主机路径历史中取出当前节点
                }
                catch
                {
                    loc = null; // 索引越界时回退为 null，由下方 continue 跳过
                }

                session.PathIndex++; // 推进索引，无论当前节点是否成功处理都继续

                if (loc == null) // 索引越界或数据异常导致快照节点为 null
                {
                    continue; // 跳过无效节点，继续处理下一个
                }

                int act; // 节点所在 Act（幕）
                int x;   // 节点在地图中的 X 坐标（层）
                int y;   // 节点在地图中的 Y 坐标（列）
                if (!TryParseNodeKey(loc.NodeId, out act, out x, out y, out _)) // 尝试从 NodeId 解析 Act:X:Y；解析失败时回退到坐标字段
                {
                    act = 0; // Act 解析失败时回退为 0（忽略 Act 过滤）
                    x = loc.X; // 回退到快照中的 X 坐标
                    y = loc.Y; // 回退到快照中的 Y 坐标
                }

                MapNode node = TryFindNode(map, act, x, y); // 在本地地图中按坐标查找对应节点
                if (node == null) // 坐标对应节点不存在（地图结构不一致或快照过期）
                {
                    continue; // 无关节点，跳过
                }

                // EnterNode 在非强制模式下要求 Active/CrossActive；我们始终强制进入。
                // MapNode.Status setter 在游戏程序集内为 internal，因此使用反射设置。
                TrySetNodeStatus(node, MapNodeStatus.Active); // 将节点状态强制设为 Active，确保 EnterNode 允许进入

                try
                {
                    map.EnterNode(node, freeMove: true, forced: true); // 强制进入节点，触发内部路径记账和状态副作用
                }
                catch
                {
                    // TODO: 应记录异常信息，避免静默吞掉错误
                    // ignored
                }

                // 若当前是已清理的战斗节点，暂停追赶并让用户结算奖励。
                TryBeginSettlementForNode_NoThrow(map, node, session); // 检查该节点是否需要弹出奖励/结算 UI
                if (!string.IsNullOrWhiteSpace(session.PendingSettlementNodeKey)) // 若有待结算节点，本轮追赶暂停
                {
                    break; // 跳出循环，等待用户结算完成后由 PumpMainThread 再次驱动
                }
            }
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }
    #endregion

    #region 私有方法 — 结算 UI 驱动

    /// <summary>
    /// 为指定节点初始化结算挂起状态。
    /// 仅当该节点在主机已通关节点列表中且为战斗类型（Enemy/Elite/Boss）时，
    /// 才会创建 Station 实例并挂起追赶流程，等待用户完成奖励/遗物选取。
    /// 非战斗节点直接标记为已结算，跳过 UI 弹窗。
    /// </summary>
    /// <param name="map">当前游戏地图。</param>
    /// <param name="node">刚通过 EnterNode 进入的节点。</param>
    /// <param name="session">追赶会话，包含已通关节点和已结算节点集合。</param>
    private static void TryBeginSettlementForNode_NoThrow(GameMap map, MapNode node, CatchUpSession session)
    {
        try
        {
            if (map == null || node == null || session == null) // 防御性校验：参数为 null 时无法执行结算初始化
            {
                return; // 参数无效，直接退出
            }

            if (!string.IsNullOrWhiteSpace(session.PendingSettlementNodeKey)) // 已有其他节点正在挂起结算
            {
                // 当前有节点正在结算中，不允许同时挂起多个结算。
                return; // 跳过，等待当前结算完成后再处理本节点
            }

            string nodeKey = BuildNodeKey(node); // 构造节点唯一键（格式：Act:X:Y:StationType）
            if (string.IsNullOrWhiteSpace(nodeKey)) // 节点键构造失败（节点为空或异常）
            {
                return; // 无法标识节点，跳过
            }

            if (session.SettledNodeKeys.Contains(nodeKey)) // 该节点已在之前的 tick 中完成结算
            {
                return; // 已结算，无需重复处理
            }

            if (session.ClearedNodeKeys == null || !session.ClearedNodeKeys.Contains(nodeKey)) // 该节点不在主机已通关列表中
            {
                return; // 非已通关节点，跳过（主机未清理此节点，加入者也不应弹出结算 UI）
            }

            // 仅战斗节点（Enemy/Elite/Boss）拥有 RewardPanel 结算流程；其他站点类型有自己的 UI 逻辑。
            if (node.StationType != StationType.Enemy && node.StationType != StationType.EliteEnemy && node.StationType != StationType.Boss)
            {
                session.SettledNodeKeys.Add(nodeKey);
                return;
            }

            // 创建 Station 实例并预生成奖励。
            // 依赖当前 run 的 stage 创建 station（路径历史预期在当前 stage 内）。
            // Stage.CreateStation 会绑定 GameRun/Stage/Act/Level/BossId，是最小安全入口。
            GameRunController run = null;
            try
            {
                run = GameStateUtils.GetCurrentGameRun(); // 获取当前 GameRun 实例
            }
            catch
            {
                run = null; // 获取失败时回退为 null，下方 run?.CurrentStage 检查会安全退出
            }

            if (run?.CurrentStage == null) // GameRun 或当前关卡未就绪
            {
                return; // 无法创建 Station，跳过结算
            }

            Station station = null;
            try
            {
                station = run.CurrentStage.CreateStation(node); // 通过 Stage 创建与节点绑定的 Station 实例
            }
            catch
            {
                station = null; // 创建失败时回退为 null，下方 null 检查会安全退出
            }

            if (station == null) // Station 创建失败（如关卡不支持该节点类型）
            {
                return; // 无法初始化结算，跳过
            }

            try
            {
                // 奖励仅在 BattleStation 上有定义；非 BattleStation 调用 GenerateRewards 为空操作。
                (station as BattleStation)?.GenerateRewards(); // 预生成战斗奖励（卡牌/金币/遗物），供后续 RewardPanel 使用
            }
            catch
            {
                // TODO: 应记录异常信息，避免静默吞掉错误
                // ignored
            }

            session.PendingSettlementNodeKey = nodeKey; // 记录当前挂起结算的节点键，暂停追赶直到用户完成结算
            session.PendingSettlementStation = station; // 缓存 Station 实例，供 StepDriveSettlementUi 使用
            session.PendingBossExhibitShown = false; // 重置 Boss 遗物弹窗标记，允许本次结算展示
            session.PendingRewardShown = false; // 重置奖励面板标记，允许本次结算展示
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }

    /// <summary>
    /// 驱动结算 UI 流程（Boss 遗物弹窗 → RewardPanel 奖励领取）。
    /// 当有待结算节点时，依次弹窗等待用户操作；面板关闭后标记结算完成并继续追赶。
    /// 仅加入者需要结算 UI；房主已在本地完成结算。
    /// </summary>
    /// <param name="run">当前 GameRun。</param>
    /// <param name="session">追赶会话，包含挂起结算状态。</param>
    /// <returns>true 表示有面板正在展示（暂停追赶）；false 表示结算完成或无需结算（可继续追赶）。</returns>
    private bool StepDriveSettlementUi_NoThrow(GameRunController run, CatchUpSession session)
    {
        try
        {
            if (run == null || session == null) // 防御性校验：参数为 null 时无法驱动结算 UI
            {
                return false; // 参数无效，无需暂停追赶
            }

            if (string.IsNullOrWhiteSpace(session.PendingSettlementNodeKey) || session.PendingSettlementStation == null) // 无挂起结算节点或站点实例
            {
                return false; // 无需结算，允许追赶继续推进
            }

            // 仅加入者需要进行奖励结算；房主已在本地完成了自己的结算流程。
            if (!IsJoinerConnected_NoThrow()) // 当前是房主或尚未连接时，直接标记为已结算并跳过 UI
            {
                session.SettledNodeKeys.Add(session.PendingSettlementNodeKey); // 将当前节点加入已结算集合，防止下次再进入
                ClearPendingSettlement_NoThrow(session); // 清空待结算状态，允许追赶继续推进
                return false; // 无需弹出 UI，返回 false 表示本轮追赶可继续
            }

            // UI 必须已完成初始化，否则弹窗会异常或无法显示
            if (!UiManager.IsInitialized) // UI 管理器尚未就绪（如游戏还在加载中）
            {
                return true; // 返回 true 暂停追赶，等待 UI 就绪后再继续
            }

            // 若有结算相关面板正处于打开状态，等待其关闭后再继续。
            try
            {
                var rp = UiManager.GetPanel<RewardPanel>(); // 获取奖励面板实例
                if (rp != null && rp.gameObject != null && rp.gameObject.activeSelf) // 面板存在且处于激活状态
                {
                    return true; // 面板未关闭，返回 true 暂停追赶
                }
            }
            catch
            {
                // TODO: 应记录异常信息，避免静默吞掉错误
                // ignored
            }

            try
            {
                var bp = UiManager.GetPanel<BossExhibitPanel>(); // 获取 Boss 遗物面板实例
                if (bp != null && bp.gameObject != null && bp.gameObject.activeSelf) // 面板存在且处于激活状态
                {
                    return true; // 面板未关闭，返回 true 暂停追赶
                }
            }
            catch
            {
                // TODO: 应记录异常信息，避免静默吞掉错误
                // ignored
            }

            // Boss 节点：先显示 Boss 遗物奖励（与 GameMaster.EndStationFlow 顺序一致）。
            if (!session.PendingBossExhibitShown) // Boss 遗物弹窗尚未显示
            {
                if (session.PendingSettlementStation is BossStation bossStation) // 待结算结点是 Boss 节点
                {
                    try
                    {
                        bossStation.GenerateBossRewards(); // 预生成 Boss 战利品（遗物列表）
                        Exhibit[] bossRewards = bossStation.BossRewards; // 获取已生成的 Boss 遗物数组
                        if (bossRewards != null && bossRewards.Length > 0) // 有 Boss 遗物可展示
                        {
                            UiManager.GetPanel<BossExhibitPanel>()?.Show(bossRewards); // 弹出 Boss 遗物选择面板
                            session.PendingBossExhibitShown = true; // 标记已显示，避免重复弹出
                            return true; // 面板已打开，返回 true 暂停追赶
                        }
                    }
                    catch
                    {
                        // TODO: 应记录异常信息，避免静默吞掉错误
                        // ignored
                    }
                }

                // 非 Boss 节点，或没有 Boss 奖励需要显示。
                session.PendingBossExhibitShown = true; // 标记为已处理，跳过 Boss 遗物阶段
            }

            // 显示主 RewardPanel 以领取车站奖励。
            if (!session.PendingRewardShown) // 奖励面板尚未显示
            {
                try
                {
                    UiManager.GetPanel<RewardPanel>()?.Show(new ShowRewardContent // 弹出车站奖励面板（卡牌/金币/遗物等）
                    {
                        RewardType = RewardType.Station, // 奖励类型为车站奖励（区别于战斗奖励）
                        Station = session.PendingSettlementStation, // 传入车站实例以展示对应奖励
                        ShowNextButton = true, // 显示"下一张"按钮，允许玩家浏览所有奖励后关闭
                    });

                    session.PendingRewardShown = true; // 标记已显示，避免重复弹出
                    return true; // 面板已打开，返回 true 暂停追赶
                }
                catch
                {
                    // 若显示失败，不应永久阻塞追赶流程。
                    session.SettledNodeKeys.Add(session.PendingSettlementNodeKey); // 显示失败时直接标记为已结算，防止永久卡住
                    //TODO: 结算失败奖励五个金币
                    ClearPendingSettlement_NoThrow(session); // 清空待结算状态，允许追赶继续
                    return false; // 返回 false 表示追赶可继续推进
                }
            }

            // 执行到此处说明面板已关闭且奖励已展示完毕。
            session.SettledNodeKeys.Add(session.PendingSettlementNodeKey); // 将节点加入已结算集合
            ClearPendingSettlement_NoThrow(session); // 清空待结算状态，准备处理下一个节点
            return false; // 结算完成，返回 false 允许追赶继续推进
        }
        catch
        {
            return false; // 异常时返回 false，不阻塞主线程驱动循环
        }
    }
    #endregion

    #region 私有方法 — 节点状态应用与生命周期管理

    /// <summary>
    /// 清理追赶会话中挂起的结算状态。
    /// 结算完成后调用，重置 PendingSettlementNodeKey、PendingSettlementStation 及 UI 标记。
    /// </summary>
    /// <param name="session">当前追赶会话。</param>
    private static void ClearPendingSettlement_NoThrow(CatchUpSession session)
    {
        try
        {
            if (session == null) // 防御性校验：会话为 null 时无需清理
            {
                return; // 无有效会话，直接退出
            }

            session.PendingSettlementNodeKey = string.Empty; // 重置挂起结算节点键，标记当前无等待结算
            session.PendingSettlementStation = null; // 清空挂起结算站点引用，释放内存
            session.PendingBossExhibitShown = false; // 重置 Boss 遗物展示标记，允许后续节点再次触发展示
            session.PendingRewardShown = false; // 重置奖励面板展示标记，允许后续节点再次触发展示
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }

    /// <summary>
    /// 将节点实例转换为唯一键字符串。
    /// 格式："Act:X:Y:StationType"，与 TryParseNodeKey 的解析规则对应。
    /// </summary>
    /// <param name="node">目标节点。</param>
    /// <returns>节点唯一键字符串；节点为 null 或异常时返回空字符串。</returns>
    private static string BuildNodeKey(MapNode node)
    {
        try
        {
            if (node == null) // 防御性校验：节点为 null 时无法构造键
            {
                return string.Empty; // 空节点回退为空字符串
            }

            return $"{node.Act}:{node.X}:{node.Y}:{node.StationType}"; // 按 Act:X:Y:StationType 格式拼接唯一键
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            return string.Empty; // 异常时保守返回空字符串，防止调用方使用无效键
        }
    }

    /// <summary>
    /// 安全判断当前客户端是否已连接且非主机。
    /// 用于防止在断线或主机端执行追赶逻辑。
    /// </summary>
    /// <returns>true 表示加入者已连接且非主机；false 表示未连接或本身就是主机。</returns>
    private bool IsJoinerConnected_NoThrow()
    {
        try
        {
            IServiceProvider sp = ModService.ServiceProvider; // 获取插件服务容器
            INetworkClient client = sp?.GetService<INetworkClient>(); // 从服务容器中获取网络客户端实例
            if (client == null || !client.IsConnected) // 客户端未初始化或当前未连接到服务器
            {
                return false; // 未连接，直接返回 false
            }

            NetworkIdentityTracker.EnsureSubscribed(client); // 确保网络身份追踪器已订阅客户端连接状态
            return !NetworkIdentityTracker.GetSelfIsHost(); // 返回"自身不是主机"的判断结果
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            return false; // 异常时保守返回 false，避免在错误状态下执行追赶
        }
    }

    /// <summary>
    /// 按预算逐步同步主机节点状态到本地地图。
    /// 每帧只处理 budget 个节点状态，防止单帧卡顿。
    /// </summary>
    /// <param name="map">当前游戏地图。</param>
    /// <param name="session">追赶会话，跟踪 NodeStateIndex 与节点状态对列表。</param>
    /// <param name="nodeStatesBudget">本次调用允许处理的最大节点状态数。</param>
    private static void StepApplyNodeStates(GameMap map, CatchUpSession session, int nodeStatesBudget)
    {
        try
        {
            if (map == null || session == null) // 防御性校验：参数为 null 时无法执行状态同步
            {
                return; // 参数无效，直接退出
            }

            if (nodeStatesBudget <= 0) // 预算为零或负数，本次无需处理
            {
                return; // 无预算，直接退出
            }

            var list = session.NodeStatePairs; // 获取会话中缓存的节点状态对列表
            int total = list?.Count ?? 0; // 计算节点状态对总数；若对象为 null 则回退为 0
            if (total <= 0) // 主机未发送节点状态或列表为空
            {
                session.NodeStateIndex = 0; // 重置索引到起始位置
                return; // 无节点状态需要同步
            }

            for (int i = 0; i < nodeStatesBudget && session.NodeStateIndex < total; i++) // 按预算循环处理，同时确保不越界
            {
                KeyValuePair<string, string> kv; // 当前节点状态对（Key=节点标识，Value=状态字符串）
                try
                {
                    kv = list[session.NodeStateIndex]; // 从列表中按当前索引读取节点状态对
                }
                catch
                {
                    session.NodeStateIndex++; // 索引越界或列表被修改时跳过当前位置
                    continue; // 继续处理下一个节点状态
                }

                session.NodeStateIndex++; // 成功读取后推进索引，标记该节点状态已处理

                string nodeKey = kv.Key; // 节点唯一标识字符串（如 "Act#0#X#1#Y#2"）
                string state = kv.Value; // 节点状态字符串（如 "Active"、"CrossActive"）

                if (!TryParseNodeKey(nodeKey, out int act, out int x, out int y, out string stationType)) // 解析节点标识获取坐标和房间类型
                {
                    continue; // 解析失败，无法定位节点，跳过
                }

                MapNode node = TryFindNode(map, act, x, y); // 在本地地图中查找对应节点
                if (node == null) // 坐标对应节点不存在
                {
                    continue; // 无关节点，跳过
                }

                if (!string.IsNullOrWhiteSpace(stationType) &&
                    !string.Equals(node.StationType.ToString(), stationType, StringComparison.Ordinal))
                {
                    // 坐标是主键；StationType 不一致仅记录日志，不阻断同步流程。
                }

                if (!Enum.TryParse(state, out MapNodeStatus status)) // 将字符串状态解析为枚举值
                {
                    continue; // 状态字符串非法或不受支持，跳过
                }

                TrySetNodeStatus(node, status); // 将解析后的状态应用到本地节点
            }
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }
    #endregion

    #region 私有方法 — 房间状态请求与日志辅助

    /// <summary>
    /// 地图状态应用完成后清理所有暂存数据。
    /// 在锁内操作，确保与 PumpMainThread 并发写互斥。
    /// </summary>
    private void ClearPendingAfterApplied_NoThrow()
    {
        try
        {
            lock (_lock) // 与 PumpMainThread 并发写互斥，防止竞态条件
            {
                _pendingSnapshot = null; // 清空已应用完毕的快照引用，释放内存
                _pendingReceivedAtUtcTicks = 0; // 重置接收时间戳，允许接收下一个快照
                _pendingCheckpointId = string.Empty; // 清空检查点标识
                _session = null; // 销毁追赶会话，结束本次追赶生命周期
            }
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }

    /// <summary>
    /// 标记当前快照已成功应用。
    /// 在锁内设置 _applied 标志，供 PumpMainThread 判断是否可以清理。
    /// </summary>
    private void MarkApplied()
    {
        lock (_lock) // 与 PumpMainThread 并发读互斥，确保状态一致性
        {
            _applied = true; // 标记本次快照已完整应用，主循环可据此触发清理
        }
    }

    /// <summary>
    /// 地图状态应用完成后向主机请求当前房间状态。
    /// 用于让战斗/事件补丁能复用最后进入的房间数据。
    /// </summary>
    /// <param name="snapshot">已应用的完整状态快照。</param>
    private void TryRequestRoomStateAfterMapApplied_NoThrow(FullStateSnapshot snapshot)
    {
        try
        {
            if (snapshot?.MapState?.CurrentLocation == null) // 防御性校验：快照或当前位置缺失时无法请求房间状态
            {
                return; // 参数无效，直接退出
            }

            string checkpointId = snapshot.MapState.LastCheckpointId ?? string.Empty; // 提取主机快照中的检查点标识
            if (string.IsNullOrWhiteSpace(checkpointId)) // 检查点标识为空（主机可能未生成或处于初始状态）
            {
                // 仍允许无检查点的情况下请求，但使用占位符避免重复 spam。
                checkpointId = "<no-checkpoint>"; // 用固定占位符代替空值，保证后续去重逻辑可用
            }

            lock (_lock) // 与并发写互斥，防止重复请求同一检查点
            {
                if (string.Equals(_lastRoomStateRequestedCheckpointId, checkpointId, StringComparison.Ordinal)) // 检查该检查点是否已请求过
                {
                    return; // 同一检查点已请求过，避免重复 spam
                }

                _lastRoomStateRequestedCheckpointId = checkpointId; // 记录本次请求的检查点标识
            }

            LocationSnapshot loc = snapshot.MapState.CurrentLocation; // 提取主机当前位置快照
            int act; // 节点所在 Act（幕）
            int x;   // 节点 X 坐标（层）
            int y;   // 节点 Y 坐标（列）
            string stationType; // 房间类型字符串

            if (!TryParseNodeKey(loc.NodeId, out act, out x, out y, out stationType)) // 尝试从 NodeId 解析坐标和类型；失败时回退
            {
                act = 0; // Act 解析失败时回退为 0
                x = loc.X; // 回退到快照中的 X 坐标
                y = loc.Y; // 回退到快照中的 Y 坐标
                stationType = loc.NodeType ?? string.Empty; // 回退到快照中的节点类型
            }

            if (string.IsNullOrWhiteSpace(stationType)) // 回退后房间类型仍为空
            {
                stationType = "Unknown"; // 使用 Unknown 作为默认类型，避免 BuildRoomKey 失败
            }

            // 保持 RoomSyncManager 的本地辅助数据对齐，使战斗补丁可以复用最后进入的房间。
            _roomSyncManager.SetLastEnteredNode(act, x, y, stationType); // 向房间同步管理器注册最后进入的节点信息

            string roomKey = RoomSyncManager.BuildRoomKey(act, x, y, stationType); // 构造房间唯一标识字符串
            long knownVersion = _roomSyncManager.TryGetClientRoomState(roomKey)?.RoomVersion ?? 0; // 获取本地已缓存的房间状态版本号
            _roomSyncManager.RequestRoomState(roomKey, knownVersion); // 向主机发起房间状态请求，携带本地已知版本号以便主机做增量推送
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }

    /// <summary>
    /// 记录地图种子未对齐的警告日志，并做 5 秒节流去重。
    /// 防止同一检查点在短时间内重复 spam 日志。
    /// </summary>
    /// <param name="checkpointId">当前未对齐的检查点标识。</param>
    private void MaybeLogSeedMismatch(string checkpointId)
    {
        try
        {
            long now = DateTime.UtcNow.Ticks; // 获取当前 UTC 时间戳（Ticks）
            const long logIntervalTicks = TimeSpan.TicksPerSecond * 5; // 日志节流间隔：5 秒 = 5 * 1000 万 Ticks

            lock (_lock) // 与并发写互斥，确保日志节流状态一致
            {
                if (string.Equals(_lastSeedMismatchCheckpointId, checkpointId ?? string.Empty, StringComparison.Ordinal) && // 同一检查点
                    now - _lastSeedMismatchLogAtUtcTicks < logIntervalTicks) // 距离上次记录不足 5 秒
                {
                    return; // 满足节流条件，跳过本次日志记录
                }

                _lastSeedMismatchCheckpointId = checkpointId ?? string.Empty; // 更新最后记录的检查点标识
                _lastSeedMismatchLogAtUtcTicks = now; // 更新最后日志时间戳
            }

            _logger?.LogWarning($"[MapCatchUp] Pending MapState waiting for map seed alignment: checkpoint={checkpointId}"); // 输出种子未对齐警告日志，便于排查地图不一致问题
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }

    /// <summary>
    /// 判断本地地图种子是否与主机快照中的种子一致。
    /// 种子不一致意味着地图结构不同，此时应用节点状态可能导致错乱。
    /// </summary>
    /// <param name="run">当前 GameRun。</param>
    /// <param name="mapState">主机快照中的地图状态。</param>
    /// <returns>true 表示种子一致或无法获取种子（保守通过）；false 表示种子明显不同。</returns>
    private static bool IsLikelySameMapSeed(GameRunController run, MapStateSnapshot mapState)
    {
        try
        {
            if (mapState == null || mapState.MapSeedUlong == null) // 主机快照未提供种子信息
            {
                return true; // 无法验证时保守返回 true，避免阻塞正常流程
            }

            ulong? local = run?.CurrentStage?.MapSeed; // 从本地 GameRun 获取当前阶段的地图种子
            if (local == null) // 本地种子尚未生成或当前阶段为 null
            {
                return true; // 无法验证时保守返回 true
            }

            return local.Value == mapState.MapSeedUlong.Value; // 比较本地种子与主机种子是否完全一致
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            return true; // 异常时保守返回 true，防止阻塞正常流程
        }
    }
    #endregion

    #region 私有方法 — 路径与状态一次性应用

    /// <summary>
    /// 一次性完整应用地图状态（路径历史、节点状态、当前位置）。
    /// 供旧版同步或测试路径使用，增量追赶优先使用 <see cref="TryApplyPendingToCurrentRun"/>。
    /// </summary>
    /// <param name="run">当前 GameRun。</param>
    /// <param name="mapState">快照中的地图状态。</param>
    private void ApplyMapStateToRun(GameRunController run, MapStateSnapshot mapState)
    {
        GameMap map = run.CurrentMap; // 从 GameRun 获取当前地图实例
        if (map == null) // 地图尚未生成（如游戏还在加载中）
        {
            return; // 地图未就绪，无法应用快照
        }

        // 0) 路径历史：通过 GameMap.EnterNode(forced=true) 重建路径
        // 使内部副作用（Visiting/Visited/Passed 及 _path 记账）保持一致。
        // RoomStateSyncPatch 会忽略 forced EnterNode，因此不会触发多余的 RoomStateRequest。
        TryApplyPathByEnterNode(map, mapState.PathHistory); // 一次性重建完整路径历史（不增量）

        // 1) 节点状态：按房主的 node.Status.ToString() 反向设置。
        if (mapState.NodeStates != null && mapState.NodeStates.Count > 0) // 快照包含节点状态字典时逐条应用
        {
            foreach (KeyValuePair<string, string> kv in mapState.NodeStates) // 遍历房主下发的所有节点状态键值对
            {
                string nodeKey = kv.Key; // 节点唯一键（格式：Act:X:Y:StationType）
                string state = kv.Value; // 节点状态字符串（如 "Active"、"Cleared"、"Visited"）

                if (!TryParseNodeKey(nodeKey, out int act, out int x, out int y, out string stationType)) // 解析节点键为坐标和类型
                {
                    continue; // 键格式非法，跳过该节点
                }

                MapNode node = TryFindNode(map, act, x, y); // 在本地地图中按坐标查找对应节点
                if (node == null) // 坐标对应节点不存在
                {
                    continue; // 无关节点，跳过
                }

                if (!string.IsNullOrWhiteSpace(stationType) &&
                    !string.Equals(node.StationType.ToString(), stationType, StringComparison.Ordinal)) // 快照中的 StationType 与本地节点不一致
                {
                    // 坐标是主键；StationType 不一致仅记录，不阻断。
                }

                if (!Enum.TryParse(state, out MapNodeStatus status)) // 将状态字符串反序列化为枚举值
                {
                    continue; // 未知状态字符串，跳过
                }

                TrySetNodeStatus(node, status); // 通过反射设置节点状态
            }
        }

        // 2) 路径已在上面通过 EnterNode 重建；这里不再直接写 _path。

        // 3) 当前位置（尽力设置 VisitingNode；失败则不阻断）。
        TryApplyCurrentLocation(map, mapState); // 将本地 VisitingNode 对齐到房主当前所在节点
    }
    #endregion

    #region 私有方法 — 节点工具方法

    /// <summary>
    /// 将节点唯一键字符串解析为坐标和房间类型。
    /// 期望格式："Act:X:Y:StationType"（至少 4 段，冒号分隔）。
    /// </summary>
    /// <param name="nodeKey">节点唯一键字符串。</param>
    /// <param name="act">解析出的 Act（幕），失败时输出 0。</param>
    /// <param name="x">解析出的 X 坐标（层），失败时输出 0。</param>
    /// <param name="y">解析出的 Y 坐标（列），失败时输出 0。</param>
    /// <param name="stationType">解析出的房间类型，失败时输出空字符串。</param>
    /// <returns>true 表示解析成功；false 表示格式非法或数值解析失败。</returns>
    private static bool TryParseNodeKey(string nodeKey, out int act, out int x, out int y, out string stationType)
    {
        act = 0; // 默认回退值：Act
        x = 0;   // 默认回退值：X 坐标
        y = 0;   // 默认回退值：Y 坐标
        stationType = string.Empty; // 默认回退值：房间类型

        if (string.IsNullOrWhiteSpace(nodeKey)) // 防御性校验：空或空白键无法解析
        {
            return false; // 输入无效，解析失败
        }

        string[] parts = nodeKey.Split(':'); // 按冒号分割键字符串
        if (parts.Length < 4) // 节点键必须包含至少 4 段（Act、X、Y、StationType）
        {
            return false; // 格式不符合预期，解析失败
        }

        if (!int.TryParse(parts[0], out act) || !int.TryParse(parts[1], out x) || !int.TryParse(parts[2], out y)) // 将前 3 段解析为整数坐标
        {
            return false; // 数值段解析失败，键格式非法
        }

        stationType = parts[3] ?? string.Empty; // 第 4 段为房间类型；若缺失则回退为空字符串
        return true; // 解析成功
    }

    /// <summary>
    /// 在本地地图中按坐标和 Act 查找对应节点。
    /// 优先使用二维数组索引查找（O(1)），失败时回退到 AllNodes 线性搜索。
    /// </summary>
    /// <param name="map">当前游戏地图。</param>
    /// <param name="act">目标 Act（幕）；若小于等于 0 则忽略 Act 匹配。</param>
    /// <param name="x">目标 X 坐标（层）。</param>
    /// <param name="y">目标 Y 坐标（列）。</param>
    /// <returns>查找到的 MapNode；未找到或异常时返回 null。</returns>
    private static MapNode TryFindNode(GameMap map, int act, int x, int y)
    {
        try
        {
            if (map != null && x >= 0 && x < map.Levels && y >= 0 && y < map.Width) // 防御性校验：坐标在地图边界内
            {
                MapNode node = map.Nodes[x, y]; // 通过二维数组快速索引获取节点（O(1)）
                if (node != null && (act <= 0 || node.Act == act)) // 节点存在且 Act 匹配（或忽略 Act）
                {
                    return node; // 命中缓存索引，直接返回
                }
            }

            if (map?.AllNodes == null) // 地图未初始化或节点列表为空
            {
                return null; // 无节点可搜索
            }

            return act > 0 // 若提供了有效的 Act 值，则加入 Act 匹配条件
                ? map.AllNodes.FirstOrDefault(n => n != null && n.Act == act && n.X == x && n.Y == y) // 线性搜索：匹配 Act + 坐标
                : map.AllNodes.FirstOrDefault(n => n != null && n.X == x && n.Y == y); // 线性搜索：仅匹配坐标
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            return null; // 异常时保守返回 null，防止调用方使用无效节点
        }
    }

    /// <summary>
    /// 通过反射设置节点的 Status 属性。
    /// 因 MapNode.Status setter 在游戏程序集内为 internal，必须使用 Traverse 绕开访问限制。
    /// </summary>
    /// <param name="node">目标节点。</param>
    /// <param name="status">要设置的状态枚举值。</param>
    private static void TrySetNodeStatus(MapNode node, MapNodeStatus status)
    {
        try
        {
            Traverse.Create(node).Property("Status").SetValue(status); // 通过 HarmonyLib Traverse 反射设置 Status 属性
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }

    /// <summary>
    /// 直接覆写地图内部 _path 列表以重建路径历史（低侵入式路径同步）。
    /// 不触发 EnterNode，仅保证路径节点顺序与主机一致，供结算或回放使用。
    /// </summary>
    /// <param name="map">当前游戏地图。</param>
    /// <param name="pathHistory">主机路径历史列表。</param>
    private static void TryApplyPath(GameMap map, List<LocationSnapshot> pathHistory)
    {
        try
        {
            if (map == null) // 防御性校验：地图实例为 null 时无法重建路径
            {
                return; // 参数无效，直接退出
            }

            var list = Traverse.Create(map).Field("_path").GetValue<List<MapNode>>(); // 通过反射获取地图内部 _path 列表
            if (list == null) // 反射失败或字段类型不匹配导致 list 为 null
            {
                return; // 无法写入路径，直接退出
            }

            list.Clear(); // 清空本地路径，确保后续写入的是完整主机路径

            if (pathHistory == null || pathHistory.Count == 0) // 主机路径为空或 null
            {
                return; // 无路径历史，无需重建
            }

            foreach (LocationSnapshot loc in pathHistory) // 遍历主机路径历史中的所有节点
            {
                if (loc == null) // 路径中某节点快照为 null（协议异常或数据缺失）
                {
                    continue; // 跳过无效节点
                }

                int act; // 节点所在 Act（幕）
                int x;   // 节点 X 坐标（层）
                int y;   // 节点 Y 坐标（列）
                if (!TryParseNodeKey(loc.NodeId, out act, out x, out y, out _)) // 尝试从 NodeId 解析坐标；失败时回退
                {
                    act = 0; // Act 解析失败时回退为 0
                    x = loc.X; // 回退到快照中的 X 坐标
                    y = loc.Y; // 回退到快照中的 Y 坐标
                }

                MapNode node = TryFindNode(map, act, x, y); // 在本地地图中查找对应节点
                if (node != null) // 找到有效节点
                {
                    list.Add(node); // 将节点追加到 _path 列表，重建路径顺序
                }
            }
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }

    /// <summary>
    /// 一次性完整重建路径历史（旧版同步入口）。
    /// 会清空现有路径并强制 EnterNode 遍历所有历史节点。
    /// </summary>
    /// <param name="map">当前游戏地图。</param>
    /// <param name="pathHistory">主机路径历史列表。</param>
    private static void TryApplyPathByEnterNode(GameMap map, List<LocationSnapshot> pathHistory)
    {
        try
        {
            if (map == null) // 防御性校验：地图实例为 null 时无法重建路径
            {
                return; // 参数无效，直接退出
            }

            // 清除现有路径，避免重复追加导致路径错乱。
            try
            {
                var list = Traverse.Create(map).Field("_path").GetValue<List<MapNode>>(); // 通过反射获取地图内部 _path 列表
                list?.Clear(); // 清空原有路径，防止追加产生重复节点
            }
            catch
            {
                // TODO: 应记录异常信息，避免静默吞掉错误
                // ignored
            }

            // 重置 VisitingNode，防止首个 EnterNode 误将无关节点标记为已访问。
            try
            {
                Traverse.Create(map).Property("VisitingNode").SetValue(null); // 通过反射重置 VisitingNode 为 null
            }
            catch
            {
                // TODO: 应记录异常信息，避免静默吞掉错误
                // ignored
            }

            if (pathHistory == null || pathHistory.Count == 0) // 主机路径为空或 null
            {
                return; // 无路径历史，无需重建
            }

            foreach (LocationSnapshot loc in pathHistory) // 遍历主机路径历史中的所有节点
            {
                if (loc == null) // 路径中某节点快照为 null（协议异常或数据缺失）
                {
                    continue; // 跳过无效节点
                }

                int act; // 节点所在 Act（幕）
                int x;   // 节点 X 坐标（层）
                int y;   // 节点 Y 坐标（列）
                if (!TryParseNodeKey(loc.NodeId, out act, out x, out y, out _)) // 尝试从 NodeId 解析坐标；失败时回退
                {
                    act = 0; // Act 解析失败时回退为 0
                    x = loc.X; // 回退到快照中的 X 坐标
                    y = loc.Y; // 回退到快照中的 Y 坐标
                }

                MapNode node = TryFindNode(map, act, x, y); // 在本地地图中查找对应节点
                if (node == null) // 坐标对应节点不存在
                {
                    continue; // 无关节点，跳过
                }

                // EnterNode 在非强制模式下要求 Active/CrossActive；我们始终强制进入。
                // MapNode.Status setter 在游戏程序集内为 internal，因此使用反射设置。
                try
                {
                    Traverse.Create(node).Property("Status").SetValue(MapNodeStatus.Active); // 强制设置节点状态为 Active，确保 EnterNode 允许进入
                }
                catch
                {
                    // TODO: 应记录异常信息，避免静默吞掉错误
                    // ignored
                }

                try
                {
                    map.EnterNode(node, freeMove: true, forced: true);
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    /// 同步当前所在节点（VisitingNode）到本地地图。
    /// 用于追赶完成后将玩家位置固定在主机当前节点，确保地图显示一致。
    /// </summary>
    /// <param name="map">当前游戏地图。</param>
    /// <param name="mapState">包含 CurrentLocation 的主机地图状态快照。</param>
    private static void TryApplyCurrentLocation(GameMap map, MapStateSnapshot mapState)
    {
        try
        {
            if (map == null || mapState?.CurrentLocation == null) // 防御性校验：地图或当前位置快照缺失
            {
                return; // 参数无效，直接退出
            }

            int act; // 节点所在 Act（幕）
            int x;   // 节点 X 坐标（层）
            int y;   // 节点 Y 坐标（列）
            if (!TryParseNodeKey(mapState.CurrentLocation.NodeId, out act, out x, out y, out _)) // 尝试从 NodeId 解析坐标；失败时回退
            {
                act = 0; // Act 解析失败时回退为 0
                x = mapState.CurrentLocation.X; // 回退到快照中的 X 坐标
                y = mapState.CurrentLocation.Y; // 回退到快照中的 Y 坐标
            }

            MapNode node = TryFindNode(map, act, x, y); // 在本地地图中查找对应节点
            if (node == null) // 坐标对应节点不存在
            {
                return; // 未找到当前位置节点，无法同步
            }

            // VisitingNode 的 setter 在游戏程序集内为私有，因此使用反射/Traverse 尽力设置。
            try
            {
                Traverse.Create(map).Property("VisitingNode").SetValue(node); // 强制设置 VisitingNode 为目标节点，使地图高亮当前位置
            }
            catch
            {
                // TODO: 应记录异常信息，避免静默吞掉错误
                // ignored
            }
        }
        catch
        {
            // TODO: 应记录异常信息，避免静默吞掉错误
            // ignored
        }
    }
    #endregion
}
