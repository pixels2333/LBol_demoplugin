using System.Collections.Generic;

namespace NetworkPlugin.Commands;

/// <summary>
/// 命令接口 - 所有联机命令的基接口
/// 参考: 杀戮尖塔Together in Spire的命令系统
/// TODO: 实现具体命令类，如复活、设置、传送等
/// </summary>
public interface ICommand
{
    /// <summary>
    /// 命令名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>执行结果</returns>
    CommandResult Execute(string[] args, string senderId);

    /// <summary>
    /// 获取命令帮助信息
    /// </summary>
    string GetHelp();
}

/// <summary>
/// 命令执行结果
/// </summary>
public class CommandResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 返回消息
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 是否需要广播给所有玩家
    /// </summary>
    public bool Broadcast { get; set; }

    public CommandResult()
    {
        Success = true;
        Message = string.Empty;
        Broadcast = false;
    }

    public CommandResult(bool success, string message, bool broadcast = false)
    {
        Success = success;
        Message = message;
        Broadcast = broadcast;
    }

    public static CommandResult SuccessResult(string message, bool broadcast = false)
    {
        return new CommandResult(true, message, broadcast);
    }

    public static CommandResult FailureResult(string message, bool broadcast = false)
    {
        return new CommandResult(false, message, broadcast);
    }
}
