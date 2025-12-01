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
    string Name { get; }      // 命令的唯一标识符，用于命令识别和调用

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="args">命令参数</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>执行结果</returns>
    CommandResult Execute(string[] args, string senderId);    // 执行具体的命令逻辑，返回执行结果对象

    /// <summary>
    /// 获取命令帮助信息
    /// </summary>
    string GetHelp();        // 返回命令的使用说明和参数格式信息
}

/// <summary>
/// 命令执行结果
/// </summary>
public class CommandResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }     // 命令执行是否成功的状态标识

    /// <summary>
    /// 返回消息
    /// </summary>
    public string Message { get; set; }   // 命令执行的返回消息内容

    /// <summary>
    /// 是否需要广播给所有玩家
    /// </summary>
    public bool Broadcast { get; set; }   // 是否将结果广播给所有连接的玩家

    public CommandResult()
    {
        Success = true;
        Message = string.Empty;
        Broadcast = false;
    }    // 默认构造函数，设置成功状态为true

    public CommandResult(bool success, string message, bool broadcast = false)
    {
        Success = success;
        Message = message;
        Broadcast = broadcast;
    }    // 参数化构造函数，自定义设置命令执行结果

    public static CommandResult SuccessResult(string message, bool broadcast = false)
    {
        return new CommandResult(true, message, broadcast);
    }    // 创建成功结果的工厂方法

    public static CommandResult FailureResult(string message, bool broadcast = false)
    {
        return new CommandResult(false, message, broadcast);
    }    // 创建失败结果的工厂方法
}
