using System;
using System.Text;
using Yarn;

namespace LBoL.Core
{
	// Token: 0x0200005F RID: 95
	public static class MiscExtensions
	{
		// Token: 0x06000428 RID: 1064 RVA: 0x0000E790 File Offset: 0x0000C990
		public static T GetValue<T>(this IVariableStorage storage, string name)
		{
			T t;
			if (!storage.TryGetValue<T>(name, out t))
			{
				throw new ArgumentException("Cannot get variable " + name);
			}
			return t;
		}

		// Token: 0x06000429 RID: 1065 RVA: 0x0000E7BC File Offset: 0x0000C9BC
		public static T GetValueOrDefault<T>(this IVariableStorage storage, string name, T defaultValue)
		{
			T t;
			if (!storage.TryGetValue<T>(name, out t))
			{
				return defaultValue;
			}
			return t;
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x0000E7D8 File Offset: 0x0000C9D8
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
