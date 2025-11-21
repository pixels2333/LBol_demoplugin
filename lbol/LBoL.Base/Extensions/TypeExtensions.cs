using System;

namespace LBoL.Base.Extensions
{
	// Token: 0x02000024 RID: 36
	public static class TypeExtensions
	{
		// Token: 0x06000186 RID: 390 RVA: 0x00006928 File Offset: 0x00004B28
		public static bool IsSubclassOfGeneric(this Type type, Type genericType)
		{
			while (type != null && type != typeof(object))
			{
				Type type2 = (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
				if (genericType == type2)
				{
					return true;
				}
				type = type.BaseType;
			}
			return false;
		}

		// Token: 0x06000187 RID: 391 RVA: 0x00006978 File Offset: 0x00004B78
		public static bool TryGetGenericBaseTypeArguments(this Type type, Type genericType, out Type[] args)
		{
			while (type != null && type != typeof(object))
			{
				Type type2 = (type.IsGenericType ? type.GetGenericTypeDefinition() : type);
				if (genericType == type2)
				{
					args = type.GetGenericArguments();
					return true;
				}
				type = type.BaseType;
			}
			args = Array.Empty<Type>();
			return false;
		}
	}
}
