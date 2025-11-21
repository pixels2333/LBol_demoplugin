using System;
using System.Collections.Generic;
using System.Reflection;
using LBoL.Base;
using LBoL.Core.Helpers;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x02000015 RID: 21
	internal class GameEntityFormatWrapper
	{
		// Token: 0x060000C3 RID: 195 RVA: 0x00003750 File Offset: 0x00001950
		public GameEntityFormatWrapper(GameEntity entity)
		{
			this._entity = entity;
			Type type = entity.GetType();
			if (GameEntityFormatWrapper.ValueGetterTable.TryGetValue(type, ref this._valueGetter))
			{
				return;
			}
			this._valueGetter = new GameEntityFormatWrapper.ValueGetter(type);
			GameEntityFormatWrapper.ValueGetterTable.Add(type, this._valueGetter);
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x000037A2 File Offset: 0x000019A2
		protected virtual object GetArgument(string key)
		{
			return this._valueGetter.Get(this._entity, key);
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x000037B8 File Offset: 0x000019B8
		protected virtual string FormatArgument(object arg, string format)
		{
			if (arg is ManaGroup)
			{
				ManaGroup manaGroup = (ManaGroup)arg;
				return UiUtils.ManaGroupToText(manaGroup, false);
			}
			if (arg is BaseManaGroup)
			{
				return UiUtils.BaseManaGroupToText(((BaseManaGroup)arg).Value);
			}
			UnitName unitName = arg as UnitName;
			if (unitName != null)
			{
				if (format == null)
				{
					return unitName.ToString("cs");
				}
				if (!format.Contains('c'))
				{
					format += "c";
				}
				if (!format.Contains('s') && !format.Contains('f'))
				{
					format += "s";
				}
				return unitName.ToString(format);
			}
			else
			{
				if (arg is int)
				{
					int num = (int)arg;
					return GameEntityFormatWrapper.WrappedFormatNumber(num, num, format);
				}
				return GameEntityFormatWrapper.FallbackFormat(arg, format);
			}
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00003875 File Offset: 0x00001A75
		protected static string FallbackFormat(object obj, string format)
		{
			return RuntimeFormatter.FormatArgument(obj, format);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x00003880 File Offset: 0x00001A80
		protected static string WrappedFormatNumber(int baseValue, int value, string format)
		{
			Color color;
			if (value > baseValue)
			{
				color = GlobalConfig.IncreaseColor;
			}
			else if (value < baseValue)
			{
				color = GlobalConfig.DecreaseColor;
			}
			else
			{
				color = GlobalConfig.NormalColor;
			}
			if (format == null)
			{
				return UiUtils.WrapByColor(value.ToString(), color);
			}
			string text;
			if (RuntimeFormatter.TryGetSpecialSubstitude(value, format, out text))
			{
				return RuntimeFormatter.ApplySubstitude(UiUtils.WrapByColor(value.ToString(), color), text);
			}
			return UiUtils.WrapByColor(value.ToString(format, null), color);
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x000038F4 File Offset: 0x00001AF4
		public string Format(string key, string format)
		{
			return this.FormatArgument(this.GetArgument(key), format);
		}

		// Token: 0x04000081 RID: 129
		private static readonly Dictionary<Type, GameEntityFormatWrapper.ValueGetter> ValueGetterTable = new Dictionary<Type, GameEntityFormatWrapper.ValueGetter>();

		// Token: 0x04000082 RID: 130
		private readonly GameEntityFormatWrapper.ValueGetter _valueGetter;

		// Token: 0x04000083 RID: 131
		private readonly GameEntity _entity;

		// Token: 0x020001CC RID: 460
		private class ValueGetter
		{
			// Token: 0x06001001 RID: 4097 RVA: 0x0002AE54 File Offset: 0x00029054
			public ValueGetter(Type type)
			{
				PropertyInfo[] properties = type.GetProperties(4148);
				for (int i = 0; i < properties.Length; i++)
				{
					PropertyInfo property = properties[i];
					if (property.CanRead)
					{
						this._memberTable.TryAdd(property.Name, (object obj) => property.GetMethod.Invoke(obj, null));
					}
				}
				FieldInfo[] fields = type.GetFields(1076);
				for (int i = 0; i < fields.Length; i++)
				{
					FieldInfo field = fields[i];
					this._memberTable.TryAdd(field.Name, (object obj) => field.GetValue(obj));
				}
			}

			// Token: 0x06001002 RID: 4098 RVA: 0x0002AF1C File Offset: 0x0002911C
			public object Get(object obj, string key)
			{
				Func<object, object> func;
				if (this._memberTable.TryGetValue(key, ref func))
				{
					return func.Invoke(obj);
				}
				Debug.LogWarning(string.Concat(new string[]
				{
					"<",
					obj.GetType().Name,
					">.<",
					key,
					"> not found"
				}));
				return null;
			}

			// Token: 0x0400072F RID: 1839
			private readonly Dictionary<string, Func<object, object>> _memberTable = new Dictionary<string, Func<object, object>>();
		}
	}
}
