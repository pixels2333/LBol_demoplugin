using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network;

public class SyncVar<T>(string name, T initialValue)
{
    public string Name { get; } = name;
    private T _value = initialValue;
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
    public event Action<T> OnValueChanged;

    public override string ToString()
    {
        return $"SyncVar '{Name}': {Value}";
    }
}