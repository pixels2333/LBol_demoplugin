// 兼容性补丁：为 netstandard2.1 提供 IsExternalInit，以支持 record/init-only 等现代 C# 语法。
namespace System.Runtime.CompilerServices;

internal sealed class IsExternalInit
{
}

