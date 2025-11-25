using System;
using System.Collections.Generic;
using System.Reflection;
using LBoL.Base;
using LBoL.Core.Helpers;
using UnityEngine;
namespace LBoL.Core
{
	internal class GameEntityFormatWrapper
	{
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
		protected virtual object GetArgument(string key)
		{
			return this._valueGetter.Get(this._entity, key);
		}
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
		protected static string FallbackFormat(object obj, string format)
		{
			return RuntimeFormatter.FormatArgument(obj, format);
		}
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
		public string Format(string key, string format)
		{
			return this.FormatArgument(this.GetArgument(key), format);
		}
		private static readonly Dictionary<Type, GameEntityFormatWrapper.ValueGetter> ValueGetterTable = new Dictionary<Type, GameEntityFormatWrapper.ValueGetter>();
		private readonly GameEntityFormatWrapper.ValueGetter _valueGetter;
		private readonly GameEntity _entity;
		private class ValueGetter
		{
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
			private readonly Dictionary<string, Func<object, object>> _memberTable = new Dictionary<string, Func<object, object>>();
		}
	}
}
