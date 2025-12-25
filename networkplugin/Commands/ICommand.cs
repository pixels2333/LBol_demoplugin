namespace NetworkPlugin.Commands;

/// <summary>
/// 命令接口
/// 定义LBoL联机MOD中所有命令处理类必须实现的基本接口
/// 这是命令系统的核心契约，确保所有命令都有统一的执行方式和结果格式
/// </summary>
/// <remarks>
/// <para>
/// 该接口的主要作用：
/// - 定义命令执行的标准接口
/// - 确保所有命令类的一致性
/// - 支持命令系统的自动发现和管理
/// - 提供命令帮助信息的标准格式
/// </para>
///
/// <para>
/// 实现要求：
/// - 所有命令处理类必须实现此接口
/// - 使用CommandAttribute标记命令类
/// - 提供完整的帮助信息
/// - 处理各种执行情况和异常
/// </para>
///
/// <para>
/// 设计参考: 杀戮尖塔Together in Spire的命令系统架构
/// 适配了LBoL联机MOD的特定需求和多用户环境
/// </para>
///
/// <para>
/// TODO: 待实现的具体命令类：
/// - 复活命令 - 复活死亡的玩家
/// - 设置命令 - 修改游戏设置
/// - 传送命令 - 在地图间传送玩家
/// - 状态命令 - 查看游戏状态
/// - 管理员命令 - 用户权限管理
/// </para>
/// </remarks>
public interface ICommand
{
    /// <summary>
    /// 命令名称
    /// 获取命令的唯一标识符，用于命令识别和调用
    /// </summary>
    /// <remarks>
    /// 名称规范：
    /// - 应该与CommandAttribute中定义的名称一致
    /// - 使用小写字母和数字
    /// - 简洁明了，易于记忆
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// 执行命令
    /// 命令系统的核心方法，处理具体的命令逻辑和操作
    /// </summary>
    /// <param name="args">命令参数数组，包含用户输入的所有参数</param>
    /// <param name="senderId">发送命令的玩家ID，用于权限验证和操作追踪</param>
    /// <returns>命令执行结果，包含成功状态、消息和广播设置</returns>
    /// <remarks>
    /// <para>
    /// 执行流程要求：
    /// 1. 验证发送者权限（检查是否为管理员等）
    /// 2. 验证参数的有效性和数量
    /// 3. 执行具体的命令逻辑
    /// 4. 处理执行过程中的异常
    /// 5. 返回适当的执行结果
    /// </para>
    ///
    /// <para>
    /// 异常处理：
    /// - 捕获并记录所有异常
    /// - 返回用户友好的错误消息
    /// - 确保系统稳定性，不因命令错误而崩溃
    /// </para>
    ///
    /// <para>
    /// 权限检查：
    /// - 检查发送者是否有执行此命令的权限
    /// - 根据CommandAttribute中的RequireAdmin设置
    /// - 返回权限不足的错误信息
    /// </para>
    /// </remarks>
    CommandResult Execute(string[] args, string senderId);

    /// <summary>
    /// 获取命令帮助信息
    /// 返回命令的详细使用说明，包括参数说明和使用示例
    /// </summary>
    /// <returns>格式化的帮助信息字符串，用于显示给用户</returns>
    /// <remarks>
    /// <para>
    /// 帮助信息应该包含：
    /// - 命令的基本功能描述
    /// - 参数说明（必选/可选）
    /// - 使用示例
    /// - 权限要求说明
    /// - 可能的错误情况和解决方法
    /// </para>
    ///
    /// <para>
    /// 帮助信息格式示例：
    /// "复活指定玩家
    /// 用法: revive <玩家名>
    /// 权限: 仅管理员可用
    /// 示例: /revive Player123"
    /// </para>
    /// </remarks>
    string GetHelp();
}

/// <summary>
/// 命令执行结果类
/// 封装命令执行后的所有结果信息，包括成功状态、返回消息和广播设置
/// </summary>
/// <remarks>
/// <para>
/// 该类的主要作用：
/// - 统一命令执行结果的格式
/// - 提供成功/失败状态的明确标识
/// - 支持消息的广播控制
/// - 提供便捷的结果创建方法
/// </para>
///
/// <para>
/// 使用场景：
/// - 命令执行成功时返回成功结果和确认消息
/// - 命令执行失败时返回失败结果和错误信息
/// - 需要向所有玩家广播操作结果时设置Broadcast=true
/// - 只需要向发送者返回结果时设置Broadcast=false
/// </para>
///
/// <para>
/// 广播行为：
/// - Broadcast=true: 结果消息会发送给所有在线玩家
/// - Broadcast=false: 结果消息只发送给命令发送者
/// </para>
/// </remarks>
public class CommandResult
{
    /// <summary>
    /// 命令是否执行成功
    /// 标识命令的执行状态，用于UI显示和后续处理
    /// </summary>
    /// <remarks>
    /// 状态说明：
    /// - true: 命令执行成功，操作已完成
    /// - false: 命令执行失败，操作未完成
    /// - UI组件可以根据此状态显示不同的颜色或图标
    /// </remarks>
    public bool Success { get; set; }

    /// <summary>
    /// 返回消息
    /// 命令执行的详细结果信息，用于向用户显示执行结果
    /// </summary>
    /// <remarks>
    /// 消息内容规范：
    /// - 使用简洁明了的中文描述
    /// - 成功时：描述操作结果和状态变化
    /// - 失败时：说明失败原因和可能的解决方法
    /// - 避免使用技术术语，使用用户友好的语言
    /// </remarks>
    public string Message { get; set; }

    /// <summary>
    /// 是否需要广播给所有玩家
    /// 控制结果消息的发送范围，影响消息的可见性
    /// </summary>
    /// <remarks>
    /// 广播规则：
    /// - true: 消息发送给所有在线玩家
    /// - false: 消息只发送给命令发送者
    /// - 管理员操作通常设置为true，让所有玩家了解操作结果
    /// - 个人查询操作通常设置为false，保护用户隐私
    /// </remarks>
    public bool Broadcast { get; set; }

    /// <summary>
    /// 默认构造函数
    /// 创建一个默认的成功结果，不包含消息且不广播
    /// </summary>
    /// <remarks>
    /// 默认值设置：
    /// - Success = true: 默认为成功状态
    /// - Message = "": 空消息字符串
    /// - Broadcast = false: 不广播
    /// </remarks>
    public CommandResult()
    {
        // 默认设置命令执行成功
        Success = true;

        // 默认不包含返回消息
        Message = string.Empty;

        // 默认不广播结果
        Broadcast = false;
    }

    /// <summary>
    /// 完整参数构造函数
    /// 使用指定的参数创建命令执行结果
    /// </summary>
    /// <param name="success">命令是否执行成功</param>
    /// <param name="message">返回消息内容</param>
    /// <param name="broadcast">是否广播给所有玩家</param>
    /// <remarks>
    /// 参数说明：
    /// - success: 明确设置命令的执行状态
    /// - message: 提供详细的执行结果信息
    /// - broadcast: 根据操作性质决定是否广播
    /// </remarks>
    public CommandResult(bool success, string message, bool broadcast = false)
    {
        // 设置执行状态
        Success = success;

        // 设置返回消息
        Message = message ?? string.Empty;

        // 设置广播标志
        Broadcast = broadcast;
    }

    /// <summary>
    /// 创建成功结果
    /// 静态工厂方法，用于创建表示命令执行成功的结果对象
    /// </summary>
    /// <param name="message">成功消息内容</param>
    /// <param name="broadcast">是否广播，默认为false</param>
    /// <returns>表示成功结果的CommandResult对象</returns>
    /// <remarks>
    /// 使用场景：
    /// - 命令执行完成时返回成功确认
    /// - 操作状态变更时返回结果信息
    /// - 提供操作成功的反馈给用户
    /// </remarks>
    public static CommandResult SuccessResult(string message, bool broadcast = false)
    {
        return new CommandResult(true, message, broadcast);
    }

    /// <summary>
    /// 创建失败结果
    /// 静态工厂方法，用于创建表示命令执行失败的结果对象
    /// </summary>
    /// <param name="message">失败消息内容</param>
    /// <param name="broadcast">是否广播，默认为false</param>
    /// <returns>表示失败结果的CommandResult对象</returns>
    /// <remarks>
    /// 使用场景：
    /// - 参数验证失败时返回错误信息
    /// - 权限不足时返回权限错误
    /// - 执行异常时返回异常信息
    /// - 操作条件不满足时返回提示信息
    /// </remarks>
    public static CommandResult FailureResult(string message, bool broadcast = false)
    {
        return new CommandResult(false, message, broadcast);
    }    // 创建失败结果的工厂方法

    /// <summary>
    /// 获取结果的简短描述
    /// 用于日志记录和调试时快速识别结果内容
    /// </summary>
    /// <returns>包含状态和消息的简短描述</returns>
    /// <remarks>
    /// 返回格式：
    /// - 成功: "[SUCCESS] 消息内容"
    /// - 失败: "[FAILURE] 消息内容"
    /// - 消息过长时会自动截取
    /// </remarks>
    public string GetShortDescription()
    {
        string prefix = Success ? "[SUCCESS]" : "[FAILURE]";
        string shortMessage = Message.Length > 50
            ? Message.Substring(0, 50) + "..."
            : Message;

        return $"{prefix} {shortMessage}";
    }

    /// <summary>
    /// 检查结果的有效性
    /// 验证CommandResult对象是否包含必要的信息
    /// </summary>
    /// <returns>如果结果有效返回true，否则返回false</returns>
    /// <remarks>
    /// 验证标准：
    /// - Success字段必须已设置（true或false）
    /// - Message字段不能为null
    /// - 其他字段可以有合理的默认值
    /// </remarks>
    public bool IsValid()
    {
        // 检查消息是否为null
        if (Message == null)
        {
            return false;
        }

        // 检查状态是否已设置
        // (在C#中bool类型总是有值，但这可以明确表示检查意图)

        return true; // 所有基本验证通过
    }
}