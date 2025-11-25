using System;
using System.Text;
using Yarn;
namespace LBoL.Core
{
	public static class MiscExtensions
	{
		public static T GetValue<T>(this IVariableStorage storage, string name)
		{
			T t;
			if (!storage.TryGetValue<T>(name, out t))
			{
				throw new ArgumentException("Cannot get variable " + name);
			}
			return t;
		}
		public static T GetValueOrDefault<T>(this IVariableStorage storage, string name, T defaultValue)
		{
			T t;
			if (!storage.TryGetValue<T>(name, out t))
			{
				return defaultValue;
			}
			return t;
		}
		public static StringBuilder AppendWithColor(this StringBuilder builder, string color, params string[] values)
		{
			builder.Append("<color=").Append(color).Append(">");
			foreach (string text in values)
			{
				builder.Append(text);
			}
			builder.Append("</color>");
			return builder;
		}
	}
}
