using System;
namespace LBoL.Base.Extensions
{
	public static class TypeExtensions
	{
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
