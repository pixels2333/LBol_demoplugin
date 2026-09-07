using System.Text.Json;
using NetworkPlugin.Utils;
using Xunit;

namespace NetworkPlugin.Tests;

public class NetworkEventHelperTests
{
    [Fact]
    public void TryGetJsonElement_WithJsonElementPayload_ReturnsTrueAndElement()
    {
        using var doc = JsonDocument.Parse("{\"key\":\"value\"}");
        var element = doc.RootElement;

        var success = NetworkEventHelper.TryGetJsonElement(element, out var root);

        Assert.True(success);
        Assert.Equal("value", root.GetProperty("key").GetString());
    }

    [Fact]
    public void TryGetJsonElement_WithValidJsonStringPayload_ReturnsTrueAndElement()
    {
        var json = "{\"key\":\"value\"}";

        var success = NetworkEventHelper.TryGetJsonElement(json, out var root);

        Assert.True(success);
        Assert.Equal("value", root.GetProperty("key").GetString());
    }

    [Fact]
    public void TryGetJsonElement_WithInvalidStringPayload_ReturnsFalse()
    {
        var json = "not a valid json";

        var success = NetworkEventHelper.TryGetJsonElement(json, out var root);

        Assert.False(success);
        Assert.Equal(default, root);
    }

    [Fact]
    public void TryGetJsonElement_WithUnsupportedType_ReturnsFalse()
    {
        var success = NetworkEventHelper.TryGetJsonElement(12345, out var root);

        Assert.False(success);
        Assert.Equal(default, root);
    }

    [Fact]
    public void GetString_WithValidStringProperty_ReturnsStringValue()
    {
        using var doc = JsonDocument.Parse("{\"name\":\"Alice\",\"age\":30}");
        var element = doc.RootElement;

        var name = NetworkEventHelper.GetString(element, "name");
        Assert.Equal("Alice", name);
    }

    [Fact]
    public void GetString_WithNumberProperty_ReturnsRawText()
    {
        using var doc = JsonDocument.Parse("{\"age\":30}");
        var element = doc.RootElement;

        var age = NetworkEventHelper.GetString(element, "age");
        Assert.Equal("30", age);
    }

    [Fact]
    public void GetString_WithMissingOrNonPrimitiveProperty_ReturnsNull()
    {
        using var doc = JsonDocument.Parse("{\"nested\":{\"key\":\"value\"}}");
        var element = doc.RootElement;

        var missing = NetworkEventHelper.GetString(element, "missing");
        var nested = NetworkEventHelper.GetString(element, "nested");

        Assert.Null(missing);
        Assert.Null(nested);
    }

    [Fact]
    public void GetBool_WithProperty_ReturnsCorrectValueOrFallback()
    {
        using var doc = JsonDocument.Parse("{\"t\":true,\"f\":false,\"sTrue\":\"True\",\"sFalse\":\"false\",\"invalid\":\"abc\"}");
        var element = doc.RootElement;

        Assert.True(NetworkEventHelper.GetBool(element, "t"));
        Assert.False(NetworkEventHelper.GetBool(element, "f", true));
        Assert.True(NetworkEventHelper.GetBool(element, "sTrue"));
        Assert.False(NetworkEventHelper.GetBool(element, "sFalse", true));
        Assert.False(NetworkEventHelper.GetBool(element, "invalid", true));
        Assert.False(NetworkEventHelper.GetBool(element, "missing", false));
        Assert.True(NetworkEventHelper.GetBool(element, "missing", true));
    }

    [Fact]
    public void GetBool_WithElement_ReturnsCorrectValue()
    {
        using var doc = JsonDocument.Parse("[true, false, \"True\", \"false\", 123]");
        var array = doc.RootElement;

        Assert.True(NetworkEventHelper.GetBool(array[0]));
        Assert.False(NetworkEventHelper.GetBool(array[1]));
        Assert.True(NetworkEventHelper.GetBool(array[2]));
        Assert.False(NetworkEventHelper.GetBool(array[3]));
        Assert.False(NetworkEventHelper.GetBool(array[4]));
    }

    [Fact]
    public void TryGetInt_WithIntProperty_ReturnsTrueAndValue()
    {
        using var doc = JsonDocument.Parse("{\"num\":42,\"float\":42.6,\"str\":\"100\",\"invalid\":\"abc\"}");
        var element = doc.RootElement;

        Assert.True(NetworkEventHelper.TryGetInt(element, "num", out int numVal));
        Assert.Equal(42, numVal);

        Assert.True(NetworkEventHelper.TryGetInt(element, "float", out int floatVal));
        Assert.Equal(43, floatVal);

        Assert.True(NetworkEventHelper.TryGetInt(element, "str", out int strVal));
        Assert.Equal(100, strVal);

        Assert.False(NetworkEventHelper.TryGetInt(element, "invalid", out _));
        Assert.False(NetworkEventHelper.TryGetInt(element, "missing", out _));
    }

    [Fact]
    public void TryGetLong_WithLongProperty_ReturnsTrueAndValue()
    {
        using var doc = JsonDocument.Parse("{\"num\":922337203685477580,\"float\":1234567890123.4,\"str\":\"922337203685477580\",\"invalid\":\"abc\"}");
        var element = doc.RootElement;

        Assert.True(NetworkEventHelper.TryGetLong(element, "num", out long numVal));
        Assert.Equal(922337203685477580L, numVal);

        Assert.True(NetworkEventHelper.TryGetLong(element, "float", out long floatVal));
        Assert.Equal(1234567890123L, floatVal);

        Assert.True(NetworkEventHelper.TryGetLong(element, "str", out long strVal));
        Assert.Equal(922337203685477580L, strVal);

        Assert.False(NetworkEventHelper.TryGetLong(element, "invalid", out _));
        Assert.False(NetworkEventHelper.TryGetLong(element, "missing", out _));
    }
}
