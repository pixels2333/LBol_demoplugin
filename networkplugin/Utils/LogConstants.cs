namespace NetworkPlugin.Utils;

/// <summary>
/// 日志工具模块常量定义
/// </summary>
public static class LogConstants
{
    /// <summary>Payload 头部预览默认截断字符数</summary>
    public const int DefaultHeadLimit = 160;

    /// <summary>超过此长度的 Payload 跳过深度解析</summary>
    public const int MaxParseLength = 20_000;

    /// <summary>普通字符串截断阈值</summary>
    public const int StringTruncateThreshold = 80;

    /// <summary>Payload 头部最大预览字符数</summary>
    public const int PayloadHeadMaxChars = 200;
}
