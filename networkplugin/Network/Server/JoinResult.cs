#nullable enable
namespace NetworkPlugin.Network.Server;

public class JoinResult
{
        public bool IsSuccess { get; set; }

        public string? ErrorMessage { get; set; }

        public static JoinResult Success()
    {
        return new JoinResult { IsSuccess = true };
    }

        public static JoinResult Failed(string errorMessage)
    {
        return new JoinResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
