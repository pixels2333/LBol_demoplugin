using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network;

/// <summary>
/// 同步变量类 - 用于网络同步的可观察变量
/// 支持泛型，可以在服务端和客户端之间自动同步状态变化
/// </summary>
/// <typeparam name="T">变量类型，支持值类型和引用类型</typeparam>
public class SyncVar<T>(string name, T initialValue)
{
    /// <summary>
    /// 同步变量的名称标识符
    /// </summary>
    public string Name { get; } = name;

    private T _value = initialValue;

    /// <summary>
    /// 同步变量的值
    /// 设置值时会自动触发网络同步（服务端）或本地事件（客户端）
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            // 只有当值实际发生改变时才触发事件和更新
            // 使用 EqualityComparer<T>.Default.Equals 以处理各种类型（包括引用类型和值类型）的比较
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                // OnValueChanged 事件将在值改变后被触发
                // 在服务端，这将触发网络发送；在客户端，这将触发本地逻辑（如UI更新）
                OnValueChanged?.Invoke(_value);
            }
        }
    }

    /// <summary>
    /// 当 SyncVar 的值发生改变时触发的事件
    /// 在服务端用于触发网络发送，在客户端用于触发本地逻辑（如UI更新）
    /// </summary>
    public event Action<T> OnValueChanged;

    /// <summary>
    /// 返回同步变量的字符串表示形式
    /// </summary>
    /// <returns>包含变量名称和值的字符串</returns>
    public override string ToString()
    {
        return $"SyncVar '{Name}': {Value}";
    }
}