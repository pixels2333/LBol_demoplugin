// 服务器公共内核日志抽象：用于在不同日志框架（BepInEx / Microsoft.Extensions.Logging）之间复用核心逻辑。
#nullable enable
using System;

namespace NetworkPlugin.Network.Server.Core;

public interface IServerLogger
{
    void Debug(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message);
    void Error(Exception ex, string message);
}

