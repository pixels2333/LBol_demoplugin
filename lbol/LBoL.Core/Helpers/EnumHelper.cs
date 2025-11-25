using System;
using System.Linq;
namespace LBoL.Core.Helpers
{
	public static class EnumHelper<T> where T : struct, Enum
	{
		public static string GetName(T value)
		{
			return Enum.GetName(typeof(T), value);
		}
		public static string[] GetNames()
		{
			return Enum.GetNames(typeof(T));
		}
		public static Type GetUnderlyingType()
		{
			return Enum.GetUnderlyingType(typeof(T));
		}
		public static T[] GetValues()
		{
			return Enumerable.ToArray<T>(Enumerable.Cast<T>(Enum.GetValues(typeof(T))));
		}
		public static T Parse(string value)
		{
			return (T)((object)Enum.Parse(typeof(T), value));
		}
		public static T Parse(string value, bool ignoreCase)
		{
			return (T)((object)Enum.Parse(typeof(T), value, ignoreCase));
		}
		public static T Max()
		{
			return Enumerable.Max<T>(EnumHelper<T>.GetValues());
		}
		public static T Min()
		{
			return Enumerable.Min<T>(EnumHelper<T>.GetValues());
		}
	}
}
