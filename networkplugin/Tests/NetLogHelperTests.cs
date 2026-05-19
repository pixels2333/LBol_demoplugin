using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class NetLogHelperTests
{
    [Fact]
    public void BuildSummary_NullJson_ReturnsEmptySafe()
    {
        var result = NetLogHelper.BuildSummary("TestType", null);
        Assert.NotNull(result);
    }

    [Fact]
    public void BuildSummary_EmptyJson_ReturnsNonNull()
    {
        var result = NetLogHelper.BuildSummary("TestType", string.Empty);
        Assert.NotNull(result);
    }

    [Fact]
    public void BuildSummary_SanitizesTokenField()
    {
        var json = "{\"EventType\":\"Test\",\"JoinToken\":\"secret123\",\"PlayerName\":\"Alice\"}";
        var result = NetLogHelper.BuildSummary("Test", json);
        // Token values should not appear in plaintext in the summary
        Assert.DoesNotContain("secret123", result, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildSummary_SanitizesReconnectTokenField()
    {
        var json = "{\"EventType\":\"Test\",\"ReconnectToken\":\"super-secret-token\"}";
        var result = NetLogHelper.BuildSummary("Test", json);
        Assert.DoesNotContain("super-secret-token", result, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildSummary_IncludesPlayerName()
    {
        var json = "{\"EventType\":\"Test\",\"PlayerName\":\"Alice\",\"Payload\":\"data\"}";
        var result = NetLogHelper.BuildSummary("Test", json);
        Assert.Contains("Alice", result, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildSummary_LongPayload_Truncated()
    {
        var longPayload = new string('x', 10_000);
        var json = $"{{\"EventType\":\"Test\",\"PlayerName\":\"Bob\",\"Payload\":\"{longPayload}\"}}";
        var result = NetLogHelper.BuildSummary("Test", json);
        // Should not contain the full 10k payload
        Assert.True(result.Length < 5000, "Summary should be significantly shorter than the full payload");
    }
}
