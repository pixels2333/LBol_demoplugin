using System;
using System.Collections.Generic;

namespace NetworkPlugin.Network.Sync;

public class SyncVar<T>(string name, T initialValue)
{
        public string Name { get; } = name;

    private T _value = initialValue;

        public T Value
    {
        get => _value;
        set
        {

            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;

                OnValueChanged?.Invoke(_value);
            }
        }
    }

        public event Action<T> OnValueChanged;

        public override string ToString()
    {
        return $"SyncVar '{Name}': {Value}";
    }
}
