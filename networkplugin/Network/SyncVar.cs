using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network;

/// <summary>
/// 泛型网络同步变量，封装一个值并在其改变时触发事件，用于在客户端和服务端之间同步状态。
/// </summary>
/// <typeparam name="T">同步变量的值类型。</typeparam>
public class SyncVar<T>(string name, T initialValue)
{
    /// <summary>同步变量的名称，用于标识和调试。</summary>
    public string Name { get; } = name;
    private T _value = initialValue;
    /// <summary>
    /// 同步变量的当前值。设置时若与旧值不同，则触发 <see cref="OnValueChanged"/> 事件。
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

    // 当 SyncVar 的值发生改变时触发的事件
    /// <summary>当 <see cref="Value"/> 发生改变时触发的事件，传递新值。</summary>
    public event Action<T> OnValueChanged;

    /// <summary>
    /// 返回同步变量的名称和当前值的字符串表示。
    /// </summary>
    public override string ToString()
    {
        return $"SyncVar '{Name}': {Value}";
    }
}