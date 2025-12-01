using System;

namespace NetworkPlugin.Commands;

/// <summary>
/// 命令特性 - 用于标记命令类和提供元数据
/// 参考: 杀戮尖塔Together in Spire的命令系统
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandAttribute(string name, string description) : Attribute
{
    /// <summary>
    /// 命令名称
    /// </summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
  // 命令名称标识符，用于命令解析和调用

    /// <summary>
    /// 命令描述
    /// </summary>
    public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));
// 命令功能的详细说明文本，用于帮助文档显示

    /// <summary>
    /// 是否需要管理员权限
    /// </summary>
    public bool RequireAdmin { get; set; }
// 权限控制标志，为true时需要管理员权限才能执行命令

    /// <summary>
    /// 参数格式说明
    /// </summary>
    public string Usage { get; set; }
// 命令参数使用格式的说明文本，指导用户正确使用命令
}
