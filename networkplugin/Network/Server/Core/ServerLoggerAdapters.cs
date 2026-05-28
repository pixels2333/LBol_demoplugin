// 服务器公共内核日志适配器：将不同日志框架统一到 IServerLogger 接口。
#nullable enable
using System;
using BepInEx.Logging;
using Microsoft.Extensions.Logging;

namespace NetworkPlugin.Network.Server.Core;

/// <summary>
/// 空日志记录器，所有日志方法均无操作
/// </summary>
public sealed class NullServerLogger : IServerLogger
{
    /// <inheritdoc />
    public void Debug(string message) { }
    /// <inheritdoc />
    public void Info(string message) { }
    /// <inheritdoc />
    public void Warn(string message) { }
    /// <inheritdoc />
    public void Error(string message) { }
    /// <inheritdoc />
    public void Error(Exception ex, string message) { }
}

/// <summary>
/// 基于 BepInEx 的日志记录器适配器
/// </summary>
public sealed class BepInExServerLogger : IServerLogger
{
    private readonly ManualLogSource? _logger;

    /// <summary>
    /// 初始化 BepInEx 日志适配器
    /// </summary>
    /// <param name="logger">BepInEx 日志源</param>
    public BepInExServerLogger(ManualLogSource? logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void Debug(string message) => _logger?.LogDebug(message);
    /// <inheritdoc />
    public void Info(string message) => _logger?.LogInfo(message);
    /// <inheritdoc />
    public void Warn(string message) => _logger?.LogWarning(message);
    /// <inheritdoc />
    public void Error(string message) => _logger?.LogError(message);
    /// <inheritdoc />
    public void Error(Exception ex, string message) => _logger?.LogError($"{message} {ex}");
}

/// <summary>
/// 基于 Microsoft.Extensions.Logging 的日志记录器适配器
/// </summary>
public sealed class MicrosoftServerLogger : IServerLogger
{
    private readonly ILogger? _logger;

    /// <summary>
    /// 初始化 Microsoft 日志适配器
    /// </summary>
    /// <param name="logger">Microsoft ILogger 实例</param>
    public MicrosoftServerLogger(ILogger? logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void Debug(string message) => _logger?.LogDebug(message);
    /// <inheritdoc />
    public void Info(string message) => _logger?.LogInformation(message);
    /// <inheritdoc />
    public void Warn(string message) => _logger?.LogWarning(message);
    /// <inheritdoc />
    public void Error(string message) => _logger?.LogError(message);
    /// <inheritdoc />
    public void Error(Exception ex, string message) => _logger?.LogError(ex, message);
}
