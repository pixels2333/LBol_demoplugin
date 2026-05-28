// 服务器公共内核日志抽象：用于在不同日志框架（BepInEx / Microsoft.Extensions.Logging）之间复用核心逻辑。
#nullable enable
using System;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 服务器日志接口抽象
/// 用于在不同日志框架（BepInEx / Microsoft.Extensions.Logging）之间复用核心逻辑
/// </summary>
public interface IServerLogger
{
    /// <summary>记录调试日志</summary>
    void Debug(string message);
    /// <summary>记录信息日志</summary>
    void Info(string message);
    /// <summary>记录警告日志</summary>
    void Warn(string message);
    /// <summary>记录错误日志</summary>
    void Error(string message);
    /// <summary>记录异常和错误日志</summary>
    void Error(Exception ex, string message);
}
