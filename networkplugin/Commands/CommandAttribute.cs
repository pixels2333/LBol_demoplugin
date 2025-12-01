using System;

namespace NetworkPlugin.Commands;

/// <summary>
/// 命令特性类
/// 用于标记和配置LBoL联机MOD中的命令处理类，提供命令的元数据信息
/// 这是一个自定义特性，用于支持命令系统的自动化发现和配置
/// </summary>
/// <remarks>
/// <para>
/// 该特性类的主要作用：
/// - 标记哪些类是命令处理类
/// - 提供命令的基本信息（名称、描述）
/// - 配置命令的权限要求和使用格式
/// - 支持命令系统的自动发现和注册
/// </para>
///
/// <para>
/// 使用方式：
/// 在命令处理类上应用此特性，例如：
/// [Command("help", "显示帮助信息")]
/// [Command("kick", "踢出指定玩家", RequireAdmin = true, Usage = "kick <玩家名>")]
/// </para>
///
/// <para>
/// 设计参考: 杀戮尖塔Together in Spire的命令系统架构
/// 适配了LBoL联机MOD的特定需求和权限管理
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandAttribute(string name, string description) : Attribute
{
    /// <summary>
    /// 命令名称
    /// 用户在聊天中输入的命令标识符，不包含命令前缀
    /// </summary>
    /// <remarks>
    /// 命令名称的规范：
    /// - 使用小写字母和数字
    /// - 不能包含空格或特殊字符
    /// - 应该简洁明了，易于记忆
    /// - 示例："help", "kick", "ban", "status"
    /// </remarks>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>
    /// 命令描述
    /// 简要说明命令的功能和用途，用于帮助系统和文档生成
    /// </summary>
    /// <remarks>
    /// 描述文本的规范：
    /// - 使用简洁明了的中文描述
    /// - 说明命令的主要功能和效果
    /// - 示例："显示所有可用命令的帮助信息"
    /// - 用于生成帮助菜单和命令列表
    /// </remarks>
    public string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    /// <summary>
    /// 是否需要管理员权限
    /// 控制命令是否只对管理员用户可用，用于权限管理和安全控制
    /// </summary>
    /// <remarks>
    /// 权限说明：
    /// - true: 只有管理员可以使用此命令
    /// - false: 所有用户都可以使用此命令
    /// - 默认值为false，表示公开命令
    /// - 管理员命令通常包括：踢出玩家、封禁用户、修改设置等
    /// </remarks>
    public bool RequireAdmin { get; set; }

    /// <summary>
    /// 参数格式说明
    /// 描述命令的参数使用方式和格式，用于帮助信息显示
    /// </summary>
    /// <remarks>
    /// 格式说明的规范：
    /// - 使用尖括号<>表示必选参数
    /// - 使用方括号[]表示可选参数
    /// - 使用管道符|表示多个选项
    /// - 示例："kick <玩家名> [理由]"
    /// - 示例："ban <玩家名> <时间> [小时|天|永久]"
    /// - 如果命令不需要参数，可以设置为空字符串或null
    /// </remarks>
    public string Usage { get; set; }

    /// <summary>
    /// 命令的完整帮助信息
    /// 自动生成的帮助文本，包含命令名称、描述和用法
    /// </summary>
    /// <returns>格式化的帮助信息字符串</returns>
    /// <remarks>
    /// 生成的帮助信息格式：
    /// 命令名称: 命令描述
    /// 用法: 命令名称 参数格式
    ///
    /// 示例输出：
    /// kick: 踢出指定玩家
    /// 用法: kick <玩家名> [理由]
    /// </remarks>
    public string GetHelpText()
    {
        var helpText = $"{Name}: {Description}";

        // 如果有参数格式说明，添加到帮助信息中
        if (!string.IsNullOrEmpty(Usage))
        {
            helpText += $"\n用法: {Name} {Usage}";
        }

        // 如果需要管理员权限，添加权限说明
        if (RequireAdmin)
        {
            helpText += "\n权限: 仅管理员可用";
        }

        return helpText;
    }

    /// <summary>
    /// 验证命令配置的有效性
    /// 检查命令配置是否符合系统规范和要求
    /// </summary>
    /// <returns>验证结果，包含是否有效和错误信息</returns>
    /// <remarks>
    /// 验证项目：
    /// - 命令名称不能为空
    /// - 命令名称格式是否正确
    /// - 描述信息是否完整
    /// - 可选：验证参数格式的语法
    /// </remarks>
    public (bool IsValid, string ErrorMessage) Validate()
    {
        // 验证命令名称
        if (string.IsNullOrWhiteSpace(Name))
        {
            return (false, "命令名称不能为空");
        }

        // 验证命令名称格式（不能包含空格或特殊字符）
        if (Name.Contains(" ") || Name.Contains("\t") || Name.Contains("\n"))
        {
            return (false, "命令名称不能包含空格或特殊字符");
        }

        // 验证描述信息
        if (string.IsNullOrWhiteSpace(Description))
        {
            return (false, "命令描述不能为空");
        }

        // 验证命令名称长度（建议不超过20个字符）
        if (Name.Length > 20)
        {
            return (false, "命令名称过长，建议不超过20个字符");
        }

        // 验证描述长度（建议不超过100个字符）
        if (Description.Length > 100)
        {
            return (false, "命令描述过长，建议不超过100个字符");
        }

        // 所有验证通过
        return (true, string.Empty);
    }
}