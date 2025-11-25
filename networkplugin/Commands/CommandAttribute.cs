using System;

namespace NetworkPlugin.Commands;

/// <summary>
/// 命令特性 - 用于标记命令类和提供元数据
/// 参考: 杀戮尖塔Together in Spire的命令系统
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// 命令名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 命令描述
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// 是否需要管理员权限
    /// </summary>
    public bool RequireAdmin { get; set; }

    /// <summary>
    /// 参数格式说明
    /// </summary>
    public string Usage { get; set; }

    public CommandAttribute(string name, string description)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}
