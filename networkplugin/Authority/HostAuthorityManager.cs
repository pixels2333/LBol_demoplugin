using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Authority;

/// <summary>
/// 主机权威管理器
/// 负责管理多人游戏中的主机权威系统，确保游戏状态的一致性和操作的合法性验证
/// 这是LBoL联机MOD的核心组件，实现了客户端-服务器架构中的权威验证机制
/// </summary>
/// <remarks>
/// <para>
/// 主要职责:
/// 1. 玩家操作验证 - 验证客户端请求是否合法，防止作弊和不一致状态
/// 2. 游戏状态同步决策 - 决定何时广播状态变更，确保所有客户端状态同步
/// 3. 冲突解决 - 处理多玩家同时操作的冲突，维护游戏逻辑的完整性
/// 4. 游戏流程控制 - 控制回合、阶段转换等关键游戏流程
/// </para>
///
/// <para>
/// 权威系统工作原理：
/// - 房主作为主机，拥有最终的游戏状态决定权
/// - 所有客户端操作需要通过主机验证后才能生效
/// - 主机验证通过后广播操作结果给所有客户端
/// - 非主机客户端将请求转发给主机处理
/// </para>
///
/// <para>
/// 典型使用场景：
/// - 玩家打牌操作的验证和同步
/// - 回合转换的权威控制
/// - 战斗状态的一致性维护
/// - 断线重连时的状态恢复
/// </para>
/// </remarks>
public class HostAuthorityManager
{
    /// <summary>
    /// 服务提供者实例，用于获取依赖注入的网络服务
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// 当前主机玩家的唯一标识符
    /// 用于识别哪个客户端拥有权威决定权
    /// </summary>
    public string HostPlayerId { get; private set; }

    /// <summary>
    /// 标识当前客户端是否为主机
    /// 主机负责验证和广播所有游戏操作
    /// </summary>
    public bool IsLocalHost { get; private set; }

    /// <summary>
    /// 权威游戏状态缓存
    /// 存储当前游戏的关键状态信息，用于验证客户端操作的合法性
    /// </summary>
    private Dictionary<string, object> _authoritativeState;

    /// <summary>
    /// 待处理的客户端请求队列
    /// 主机按顺序处理来自各个客户端的操作请求
    /// </summary>
    private Queue<ClientRequest> _pendingRequests;

    /// <summary>
    /// 已处理的请求历史记录
    /// 用于断线重连时的状态恢复和操作回放
    /// </summary>
    private List<ProcessedRequest> _requestHistory;

    /// <summary>
/// 构造函数，初始化权威管理器的所有组件
     /// 设置默认的空状态，准备接收和处理客户端请求
     /// </summary>
    public HostAuthorityManager()
    {
        // 初始化权威状态字典，用于存储游戏权威数据
        _authoritativeState = [];
        // 初始化请求队列，管理待处理的客户端操作
        _pendingRequests = new Queue<ClientRequest>();
        // 初始化请求历史列表，记录所有已处理的请求
        _requestHistory = [];
        // 设置默认值，等待主机信息初始化
        HostPlayerId = string.Empty;
        IsLocalHost = false;
    }

    /// <summary>
    /// 初始化主机权威系统
    /// 设置主机信息并准备开始处理客户端请求
    /// </summary>
    /// <param name="hostPlayerId">主机玩家的唯一标识符</param>
    /// <param name="isLocalHost">当前客户端是否为主机</param>
    /// <remarks>
    /// TODO: 需要实现的功能：
    /// 1. 实现主机选举逻辑 - 当没有指定主机时自动选举
    /// 2. 加载初始游戏状态 - 从当前游戏状态初始化权威状态
    /// 3. 设置主机变更回调 - 当主机发生变更时的处理逻辑
    /// 4. 初始化请求处理器 - 设置各种操作类型的处理器
    /// </remarks>
    public void Initialize(string hostPlayerId, bool isLocalHost)
    {
        // 设置主机基本信息
        HostPlayerId = hostPlayerId;
        IsLocalHost = isLocalHost;

        // TODO: 实现完整的初始化逻辑
        // TODO: 加载当前游戏状态到权威状态缓存
        // TODO: 注册网络事件监听器，处理主机变更事件
        // TODO: 初始化各种操作类型的验证器

        // 记录初始化完成日志
        Plugin.Logger?.LogInfo($"[HostAuthority] 权威系统初始化完成 - 主机: {hostPlayerId}, 本地主机: {isLocalHost}");
    }

    /// <summary>
    /// 处理客户端操作请求
    /// 这是主机权威系统的核心入口点，所有游戏操作都通过此方法处理
    /// </summary>
    /// <param name="request">来自客户端的操作请求对象</param>
    /// <returns>请求验证结果，包含是否成功、执行的操作等信息</returns>
    /// <remarks>
    /// 处理流程：
    /// 1. 检查当前客户端是否为主机
    /// 2. 如果是主机，直接验证并执行请求
    /// 3. 如果不是主机，将请求转发给主机处理
    /// 4. 返回处理结果给调用方
    /// </remarks>
    public RequestValidationResult ProcessClientRequest(ClientRequest request)
    {
        // TODO: 实现完整的请求处理逻辑
        // 1. 验证请求格式的完整性
        // 2. 验证发起请求的玩家权限
        // 3. 验证当前游戏状态是否允许该操作
        // 4. 检查是否存在与其他操作的冲突

        // 检查当前客户端是否为主机
        if (!IsLocalHost)
        {
            // 非主机客户端需要将请求转发给真正的主机处理
            Plugin.Logger?.LogDebug($"[HostAuthority] 非主机客户端，转发请求到主机: {request.ActionType}");
            return ForwardRequestToHost(request);
        }

        // 主机端直接处理请求
        Plugin.Logger?.LogDebug($"[HostAuthority] 主机端直接处理请求: {request.ActionType} 来自 {request.PlayerId}");
        return ValidateAndExecuteRequest(request);
    }

    /// <summary>
    /// 验证并执行请求（仅主机端调用）
    /// 执行完整的验证流程并在验证通过后执行相应的游戏操作
    /// </summary>
    /// <param name="request">待验证和执行的客户端请求</param>
    /// <returns>详细的验证结果，包含错误信息或执行的操作</returns>
    private RequestValidationResult ValidateAndExecuteRequest(ClientRequest request)
    {
        // TODO: 实现完整的四层验证逻辑

        // 初始化验证结果对象
        var validationResult = new RequestValidationResult
        {
            RequestId = request.RequestId,
            IsValid = false,                    // 默认为无效，直到验证通过
            ErrorMessage = string.Empty,
            ExecutedAction = null
        };

        try
        {
            // 验证第1层：请求格式检查
            if (!ValidateRequestFormat(request))
            {
                validationResult.ErrorMessage = "请求格式无效";
                Plugin.Logger?.LogWarning($"[HostAuthority] 请求格式验证失败: {request.RequestId}");
                return validationResult;
            }

            // 验证第2层：玩家权限检查
            if (!ValidatePlayerPermissions(request))
            {
                validationResult.ErrorMessage = "玩家权限不足";
                Plugin.Logger?.LogWarning($"[HostAuthority] 玩家权限验证失败: {request.PlayerId}");
                return validationResult;
            }

            // 验证第3层：游戏状态检查
            if (!ValidateGameStateForAction(request))
            {
                validationResult.ErrorMessage = "当前游戏状态下不允许此操作";
                Plugin.Logger?.LogWarning($"[HostAuthority] 游戏状态验证失败: {request.ActionType}");
                return validationResult;
            }

            // 验证第4层：操作冲突检查
            if (CheckActionConflict(request))
            {
                validationResult.ErrorMessage = "操作与待处理的其他操作冲突";
                Plugin.Logger?.LogWarning($"[HostAuthority] 操作冲突检查失败: {request.ActionType}");
                return validationResult;
            }

            // 所有验证通过，执行权威操作
            var executedAction = ExecuteAuthoritativeAction(request);

            // 更新验证结果为成功
            validationResult.IsValid = true;
            validationResult.ExecutedAction = executedAction;

            // 记录已处理的请求到历史
            RecordProcessedRequest(request, validationResult);

            // 将执行的操作广播给所有其他客户端
            BroadcastActionToClients(executedAction);

            // 记录成功的操作日志
            Plugin.Logger?.LogInfo($"[HostAuthority] 请求验证并执行成功: {request.ActionType} 来自 {request.PlayerId}");
        }
        catch (Exception ex)
        {
            // 捕获并记录异常，确保系统稳定性
            validationResult.ErrorMessage = $"处理请求时发生异常: {ex.Message}";
            Plugin.Logger?.LogError($"[HostAuthority] 处理请求异常: {ex.Message}\n{ex.StackTrace}");
        }

        return validationResult;
    }

    /// <summary>
    /// 转发请求给主机（非主机客户端调用）
    /// 当客户端不是主机时，需要将请求通过网络发送给主机处理
    /// </summary>
    /// <param name="request">需要转发的客户端请求</param>
    /// <returns>主机处理后的验证结果</returns>
    private RequestValidationResult ForwardRequestToHost(ClientRequest request)
    {
        // TODO: 实现完整的网络转发逻辑
        // 1. 序列化请求对象为网络传输格式
        // 2. 通过网络客户端将请求发送给主机
        // 3. 等待主机处理并返回结果
        // 4. 解析主机的响应并返回给调用方

        Plugin.Logger?.LogInfo($"[HostAuthority] 正在转发请求到主机: {request.ActionType} (请求ID: {request.RequestId})");

        // TODO: 实现实际的网络通信逻辑
        // 这里应该调用网络客户端的发送方法，并等待响应
        var networkClient = serviceProvider?.GetService<INetworkClient>();
        if (networkClient != null && networkClient.IsConnected)
        {
            // TODO: 发送请求并等待响应
            // var response = networkClient.SendRequestAndWaitForResponse("AuthorityRequest", request);
            // return ParseResponse(response);
        }

        // 临时返回默认结果，待实现实际网络通信
        return new RequestValidationResult
        {
            RequestId = request.RequestId,
            IsValid = true,
            ErrorMessage = string.Empty,
            ExecutedAction = null
        };
    }

    /// <summary>
    /// 验证请求的基本格式
    /// 检查请求对象是否包含必要的字段和有效的数据
    /// </summary>
    /// <param name="request">待验证的客户端请求</param>
    /// <returns>如果请求格式有效返回true，否则返回false</returns>
    private bool ValidateRequestFormat(ClientRequest request)
    {
        // TODO: 实现更详细的格式验证
        // 1. 检查请求ID格式是否正确
        // 2. 检查玩家ID是否在有效范围内
        // 3. 检查操作类型是否为已知类型
        // 4. 检查参数是否完整

        // 基础格式检查：确保关键字段不为空
        return !string.IsNullOrEmpty(request.RequestId) &&      // 请求ID必须存在
               !string.IsNullOrEmpty(request.PlayerId) &&       // 玩家ID必须存在
               !string.IsNullOrEmpty(request.ActionType);       // 操作类型必须存在
    }

    /// <summary>
    /// 验证玩家是否有执行该操作的权限
    /// 检查玩家的游戏状态、回合权限等
    /// </summary>
    /// <param name="request">包含玩家信息和操作类型的请求</param>
    /// <returns>如果玩家有权限返回true，否则返回false</returns>
    private bool ValidatePlayerPermissions(ClientRequest request)
    {
        // TODO: 实现完整的权限验证逻辑
        // 1. 检查是否是当前回合的玩家（对于回合制操作）
        // 2. 检查玩家是否有特殊权限（如房主权限、管理员权限）
        // 3. 检查玩家是否被禁止执行某些操作
        // 4. 检查玩家的连接状态是否正常

        // 临时返回true，待实现具体权限逻辑
        return true;
    }

    /// <summary>
    /// 验证当前游戏状态是否允许执行该操作
    /// 检查游戏阶段、资源条件、目标有效性等
    /// </summary>
    /// <param name="request">待验证的操作请求</param>
    /// <returns>如果当前状态允许该操作返回true，否则返回false</returns>
    private bool ValidateGameStateForAction(ClientRequest request)
    {
        // TODO: 实现完整的游戏状态验证
        // 1. 检查是否是正确的游戏阶段（战斗、地图、商店等）
        // 2. 检查是否有足够的资源（法力、金币、生命值等）
        // 3. 检查操作目标是否存在且有效
        // 4. 检查是否符合游戏规则的约束条件

        // 临时返回true，待实现具体状态验证逻辑
        return true;
    }

    /// <summary>
    /// 检查操作是否与其他待处理的操作冲突
    /// 防止并发操作导致游戏状态不一致
    /// </summary>
    /// <param name="request">待检查的操作请求</param>
    /// <returns>如果存在冲突返回true，否则返回false</returns>
    private bool CheckActionConflict(ClientRequest request)
    {
        // TODO: 实现完整的冲突检测逻辑
        // 1. 检查是否有针对相同目标的待处理操作
        // 2. 检查是否有互斥的操作（如同时攻击和治疗同一目标）
        // 3. 检查操作的时间顺序是否合理
        // 4. 检查资源竞争情况

        // 临时返回false（无冲突），待实现具体冲突检测逻辑
        return false;
    }

    /// <summary>
    /// 执行权威操作
    /// 根据操作类型执行相应的游戏逻辑，并生成权威操作对象
    /// </summary>
    /// <param name="request">验证通过的客户端请求</param>
    /// <returns>已执行的权威操作对象，包含操作详情</returns>
    private AuthoritativeAction ExecuteAuthoritativeAction(ClientRequest request)
    {
        // TODO: 根据不同的ActionType执行相应的游戏操作
        // 1. 打牌操作 - PlayCardAction
        // 2. 使用宝物操作 - UseExhibitAction
        // 3. 进入房间操作 - EnterRoomAction
        // 4. 选择事件选项操作 - SelectEventOptionAction
        // 5. 结束回合操作 - EndTurnAction

        // 创建权威操作对象
        var authoritativeAction = new AuthoritativeAction
        {
            ActionId = Guid.NewGuid().ToString(),                // 生成唯一操作ID
            RequestId = request.RequestId,                        // 关联原请求ID
            ActionType = request.ActionType,                      // 操作类型
            PlayerId = request.PlayerId,                          // 执行操作的玩家ID
            Parameters = request.Parameters ?? [],                // 操作参数
            Timestamp = DateTime.Now.Ticks,                       // 操作时间戳
            Source = "HostAuthority"                              // 操作来源标识
        };

        // TODO: 根据操作类型执行具体的游戏逻辑
        // switch (request.ActionType)
        // {
        //     case "PlayCard":
        //         return ExecutePlayCardAction(request, authoritativeAction);
        //     case "UseExhibit":
        //         return ExecuteUseExhibitAction(request, authoritativeAction);
        //     // ... 其他操作类型
        // }

        return authoritativeAction;
    }

    /// <summary>
    /// 记录已处理的请求到历史
    /// 用于断线重连时的状态恢复和操作历史查询
    /// </summary>
    /// <param name="request">已处理的客户端请求</param>
    /// <param name="result">处理结果</param>
    private void RecordProcessedRequest(ClientRequest request, RequestValidationResult result)
    {
        // 创建处理记录对象
        var processedRequest = new ProcessedRequest
        {
            RequestId = request.RequestId,                        // 请求ID
            PlayerId = request.PlayerId,                          // 玩家ID
            ActionType = request.ActionType,                      // 操作类型
            Parameters = request.Parameters,                      // 操作参数
            Timestamp = DateTime.Now.Ticks,                       // 处理时间戳
            Result = result                                        // 处理结果
        };

        // 添加到历史记录
        _requestHistory.Add(processedRequest);

        // TODO: 限制历史记录大小，防止内存泄漏
        // 可以保留最近N条记录，或保留最近X分钟的记录
        // if (_requestHistory.Count > MAX_HISTORY_SIZE)
        // {
        //     _requestHistory.RemoveAt(0);
        // }
    }

    /// <summary>
    /// 广播操作给所有客户端（仅主机调用）
    /// 将权威操作通过网络发送给所有连接的客户端
    /// </summary>
    /// <param name="action">需要广播的权威操作</param>
    private void BroadcastActionToClients(AuthoritativeAction action)
    {
        // 只有主机才能广播操作
        if (!IsLocalHost)
        {
            Plugin.Logger?.LogWarning($"[HostAuthority] 非主机尝试广播操作，已拒绝: {action.ActionType}");
            return;
        }

        // TODO: 实现完整的广播逻辑
        // 1. 序列化权威操作对象为网络传输格式
        // 2. 通过网络服务器将操作发送给所有连接的客户端
        // 3. 处理发送失败的情况（客户端断线等）
        // 4. 记录广播成功的日志

        var networkClient = serviceProvider?.GetService<INetworkClient>();
        if (networkClient != null && networkClient.IsConnected)
        {
            // TODO: 实际的广播逻辑
            // networkClient.SendGameEvent("AuthoritativeAction", action);
        }

        Plugin.Logger?.LogInfo($"[HostAuthority] 正在广播操作给所有客户端: {action.ActionType} (操作ID: {action.ActionId})");
    }

    /// <summary>
    /// 应用权威操作到本地游戏（客户端接收主机广播时调用）
    /// 客户端接收到主机广播的权威操作后，应用到本地游戏状态
    /// </summary>
    /// <param name="action">来自主机的权威操作</param>
    /// <remarks>
    /// 应用流程：
    /// 1. 验证操作来源（必须来自合法的主机）
    /// 2. 解析操作内容并应用到本地游戏状态
    /// 3. 更新本地权威状态缓存
    /// 4. 触发相应的UI更新和动画效果
    /// </remarks>
    public void ApplyAuthoritativeAction(AuthoritativeAction action)
    {
        // TODO: 实现完整的操作应用逻辑
        // 1. 验证操作来源（检查是否来自当前主机）
        // 2. 根据操作类型执行相应的本地游戏逻辑
        // 3. 更新本地权威状态缓存
        // 4. 处理操作应用失败的情况

        // TODO: 添加操作来源验证
        // if (action.Source != HostPlayerId)
        // {
        //     Plugin.Logger?.LogError($"[HostAuthority] 收到非主机的权威操作，已拒绝: {action.Source}");
        //     return;
        // }

        Plugin.Logger?.LogInfo($"[HostAuthority] 正在应用权威操作到本地游戏: {action.ActionType} (来源: {action.Source})");
    }

    /// <summary>
    /// 更新权威游戏状态
    /// 在游戏状态发生变化时更新缓存中的权威状态
    /// </summary>
    /// <param name="key">状态键名</param>
    /// <param name="value">状态值</param>
    public void UpdateAuthoritativeState(string key, object value)
    {
        // 更新权威状态缓存
        _authoritativeState[key] = value;

        // TODO: 可以在这里添加状态变更的事件通知
        // OnAuthoritativeStateChanged?.Invoke(key, value);

        Plugin.Logger?.LogDebug($"[HostAuthority] 更新权威状态: {key} = {value}");
    }

    /// <summary>
    /// 获取权威游戏状态
    /// 从缓存中获取指定键名的权威状态值
    /// </summary>
    /// <param name="key">状态键名</param>
    /// <returns>状态值，如果不存在则返回null</returns>
    public object GetAuthoritativeState(string key)
    {
        return _authoritativeState.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// 处理主机迁移
    /// 当原主机断开连接时，选举新主机并迁移权威系统
    /// </summary>
    /// <param name="newHostPlayerId">新主机的玩家ID</param>
    /// <remarks>
    /// 迁移流程：
    /// 1. 选举新主机（通常选择客户端ID最小或延迟最低的玩家）
    /// 2. 将当前游戏状态同步给新主机
    /// 3. 通知所有客户端主机变更信息
    /// 4. 更新本地主机状态标志
    /// </remarks>
    public void HandleHostMigration(string newHostPlayerId)
    {
        // TODO: 实现完整的主机迁移逻辑
        // 1. 实现主机选举算法（考虑网络延迟、稳定性等因素）
        // 2. 将当前权威状态和操作历史同步给新主机
        // 3. 通过网络通知所有客户端新主机信息
        // 4. 处理迁移过程中的异常情况

        // 更新主机信息
        var oldHostId = HostPlayerId;
        HostPlayerId = newHostPlayerId;
        IsLocalHost = (newHostPlayerId == GetLocalPlayerId());

        Plugin.Logger?.LogInfo($"[HostAuthority] 主机迁移完成: {oldHostId} -> {newHostPlayerId}, 本地是否主机: {IsLocalHost}");
    }

    /// <summary>
    /// 获取本地玩家ID
    /// 从网络客户端或游戏状态中获取当前本地玩家的唯一标识
    /// </summary>
    /// <returns>本地玩家ID字符串</returns>
    private string GetLocalPlayerId()
    {
        // TODO: 从网络客户端获取本地玩家ID
        // var networkClient = serviceProvider?.GetService<INetworkClient>();
        // return networkClient?.GetLocalPlayerId() ?? "unknown";

        // 临时返回固定值，待实现实际获取逻辑
        return "local_player";
    }

    /// <summary>
    /// 清理过期的历史记录
    /// 定期清理过期的请求历史，防止内存无限增长
    /// </summary>
    private void CleanupOldHistory()
    {
        // TODO: 实现历史记录清理逻辑
        // 可以选择以下策略之一：
        // 1. 保留最近N条记录（如保留最近1000条）
        // 2. 保留最近X分钟的记录（如保留最近30分钟的记录）
        // 3. 混合策略：保留最近N条 + 最近X分钟内的记录

        if (_requestHistory.Count > 1000)
        {
            // 删除最旧的记录
            _requestHistory.RemoveRange(0, _requestHistory.Count - 1000);
        }
    }

    /// <summary>
    /// 获取断线重连所需的状态快照
    /// 为断线重连的玩家提供当前游戏状态和最近的操作历史
    /// </summary>
    /// <returns>包含游戏状态和操作历史的重连快照</returns>
    public ReconnectionSnapshot GetReconnectionSnapshot()
    {
        // TODO: 实现完整的状态快照生成逻辑
        // 包括：当前游戏状态、最近的操作历史、玩家信息、主机信息等

        return new ReconnectionSnapshot
        {
            GameState = new Dictionary<string, object>(_authoritativeState), // 当前游戏状态
            RecentActions = _requestHistory.TakeLast(50).ToList(),          // 最近50个操作
            Timestamp = DateTime.Now.Ticks,                                 // 快照时间戳
            HostPlayerId = HostPlayerId                                      // 当前主机ID
        };
    }
}

// ========================================
// 数据传输对象（DTO）定义
// ========================================

/// <summary>
/// 客户端请求数据传输对象
/// 包含客户端发起游戏操作的所有必要信息
/// </summary>
public class ClientRequest
{
    /// <summary>
    /// 请求的唯一标识符，用于跟踪和避免重复处理
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 发起请求的玩家ID
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型，如"PlayCard"、"UseExhibit"、"EndTurn"等
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// 操作参数，包含执行操作所需的所有数据
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = [];

    /// <summary>
    /// 请求发起的时间戳
    /// </summary>
    public long Timestamp { get; set; }
}

/// <summary>
/// 权威操作数据传输对象
/// 主机验证通过后广播给所有客户端的操作对象
/// </summary>
public class AuthoritativeAction
{
    /// <summary>
    /// 操作的唯一标识符
    /// </summary>
    public string ActionId { get; set; } = string.Empty;

    /// <summary>
    /// 关联的原始请求ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// 执行操作的玩家ID
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 操作参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = [];

    /// <summary>
    /// 操作执行的时间戳
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// 操作来源标识（通常是主机ID或"HostAuthority"）
    /// </summary>
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// 请求验证结果数据传输对象
/// 包含请求验证的详细结果信息
/// </summary>
public class RequestValidationResult
{
    /// <summary>
    /// 关联的请求ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 请求是否验证通过
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 验证失败时的错误信息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 如果验证通过，执行的权威操作对象
    /// </summary>
    public AuthoritativeAction? ExecutedAction { get; set; }
}

/// <summary>
/// 已处理请求记录数据传输对象
/// 用于记录历史请求，支持断线重连和操作回放
/// </summary>
public class ProcessedRequest
{
    /// <summary>
    /// 请求ID
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// 玩家ID
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 操作类型
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// 操作参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = [];

    /// <summary>
    /// 处理时间戳
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// 处理结果
    /// </summary>
    public RequestValidationResult Result { get; set; } = new();
}

/// <summary>
/// 断线重连状态快照数据传输对象
/// 包含断线重连所需的所有游戏状态信息
/// </summary>
public class ReconnectionSnapshot
{
    /// <summary>
    /// 当前游戏状态字典
    /// </summary>
    public Dictionary<string, object> GameState { get; set; } = [];

    /// <summary>
    /// 最近的操作历史
    /// </summary>
    public List<ProcessedRequest> RecentActions { get; set; } = [];

    /// <summary>
    /// 快照创建时间戳
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// 当前主机玩家ID
    /// </summary>
    public string HostPlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 快照版本号，用于状态同步验证
    /// </summary>
    public int Version { get; set; }
}