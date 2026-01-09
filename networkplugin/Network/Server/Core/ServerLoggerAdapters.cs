// 服务器公共内核日志适配器：将不同日志框架统一到 IServerLogger 接口。
#nullable enable
using System;
using BepInEx.Logging;
using Microsoft.Extensions.Logging;

namespace NetworkPlugin.Network.Server.Core;

public sealed class NullServerLogger : IServerLogger
{
    public void Debug(string message) { }
    public void Info(string message) { }
    public void Warn(string message) { }
    public void Error(string message) { }
    public void Error(Exception ex, string message) { }
}

public sealed class BepInExServerLogger : IServerLogger
{
    private readonly ManualLogSource? _logger;

    public BepInExServerLogger(ManualLogSource? logger)
    {
        _logger = logger;
    }

    public void Debug(string message) => _logger?.LogDebug(message);
    public void Info(string message) => _logger?.LogInfo(message);
    public void Warn(string message) => _logger?.LogWarning(message);
    public void Error(string message) => _logger?.LogError(message);
    public void Error(Exception ex, string message) => _logger?.LogError($"{message} {ex}");
}

public sealed class MicrosoftServerLogger : IServerLogger
{
    private readonly ILogger? _logger;

    public MicrosoftServerLogger(ILogger? logger)
    {
        _logger = logger;
    }

    public void Debug(string message) => _logger?.LogDebug(message);
    public void Info(string message) => _logger?.LogInformation(message);
    public void Warn(string message) => _logger?.LogWarning(message);
    public void Error(string message) => _logger?.LogError(message);
    public void Error(Exception ex, string message) => _logger?.LogError(ex, message);
}

