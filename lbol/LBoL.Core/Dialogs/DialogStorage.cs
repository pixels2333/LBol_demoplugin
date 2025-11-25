using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using Yarn;
namespace LBoL.Core.Dialogs
{
	public sealed class DialogStorage : IVariableStorage
	{
		public void SetValue(string variableName, string stringValue)
		{
			this._storage[variableName] = stringValue;
		}
		public void SetValue(string variableName, float floatValue)
		{
			this._storage[variableName] = floatValue;
		}
		public void SetValue(string variableName, bool boolValue)
		{
			this._storage[variableName] = boolValue;
		}
		public bool TryGetValue<T>(string variableName, out T result)
		{
			object obj;
			if (!this._storage.TryGetValue(variableName, ref obj))
			{
				result = default(T);
				return false;
			}
			if (obj is T)
			{
				T t = (T)((object)obj);
				result = t;
				return true;
			}
			throw new ArgumentException(string.Format("Variable {0} is present, but is of type {1}, not {2}", variableName, obj.GetType(), typeof(T)));
		}
		public void Clear()
		{
			this._storage.Clear();
		}
		public string Save()
		{
			return new SerializerBuilder().WithEventEmitter<DialogStorage.MultilineScalarFlowStyleEmitter>(([Nullable(1)] IEventEmitter emitter) => new DialogStorage.MultilineScalarFlowStyleEmitter(emitter)).Build().Serialize(this._storage);
		}
		public static DialogStorage Restore(string yaml)
		{
			Dictionary<string, object> dictionary = new Deserializer().Deserialize<Dictionary<string, object>>(yaml);
			return new DialogStorage
			{
				_storage = dictionary
			};
		}
		private Dictionary<string, object> _storage = new Dictionary<string, object>();
		private sealed class MultilineScalarFlowStyleEmitter : ChainedEventEmitter
		{
			public MultilineScalarFlowStyleEmitter(IEventEmitter nextEmitter)
				: base(nextEmitter)
			{
			}
			public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
			{
				if (typeof(string).IsAssignableFrom(eventInfo.Source.Type) && eventInfo.Source.Value is string)
				{
					eventInfo = new ScalarEventInfo(eventInfo.Source)
					{
						Style = ScalarStyle.Literal
					};
				}
				this.nextEmitter.Emit(eventInfo, emitter);
			}
		}
	}
}
