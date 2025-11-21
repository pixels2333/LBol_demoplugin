using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using Yarn;

namespace LBoL.Core.Dialogs
{
	// Token: 0x02000129 RID: 297
	public sealed class DialogStorage : IVariableStorage
	{
		// Token: 0x06000A80 RID: 2688 RVA: 0x0001D728 File Offset: 0x0001B928
		public void SetValue(string variableName, string stringValue)
		{
			this._storage[variableName] = stringValue;
		}

		// Token: 0x06000A81 RID: 2689 RVA: 0x0001D737 File Offset: 0x0001B937
		public void SetValue(string variableName, float floatValue)
		{
			this._storage[variableName] = floatValue;
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x0001D74B File Offset: 0x0001B94B
		public void SetValue(string variableName, bool boolValue)
		{
			this._storage[variableName] = boolValue;
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x0001D760 File Offset: 0x0001B960
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

		// Token: 0x06000A84 RID: 2692 RVA: 0x0001D7C0 File Offset: 0x0001B9C0
		public void Clear()
		{
			this._storage.Clear();
		}

		// Token: 0x06000A85 RID: 2693 RVA: 0x0001D7CD File Offset: 0x0001B9CD
		public string Save()
		{
			return new SerializerBuilder().WithEventEmitter<DialogStorage.MultilineScalarFlowStyleEmitter>(([Nullable(1)] IEventEmitter emitter) => new DialogStorage.MultilineScalarFlowStyleEmitter(emitter)).Build().Serialize(this._storage);
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x0001D808 File Offset: 0x0001BA08
		public static DialogStorage Restore(string yaml)
		{
			Dictionary<string, object> dictionary = new Deserializer().Deserialize<Dictionary<string, object>>(yaml);
			return new DialogStorage
			{
				_storage = dictionary
			};
		}

		// Token: 0x04000528 RID: 1320
		private Dictionary<string, object> _storage = new Dictionary<string, object>();

		// Token: 0x0200027C RID: 636
		private sealed class MultilineScalarFlowStyleEmitter : ChainedEventEmitter
		{
			// Token: 0x06001338 RID: 4920 RVA: 0x0003416E File Offset: 0x0003236E
			public MultilineScalarFlowStyleEmitter(IEventEmitter nextEmitter)
				: base(nextEmitter)
			{
			}

			// Token: 0x06001339 RID: 4921 RVA: 0x00034178 File Offset: 0x00032378
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
