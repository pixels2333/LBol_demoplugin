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
