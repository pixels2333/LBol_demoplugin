using System;
using System.Linq;

namespace LBoL.Core.Helpers
{
	// Token: 0x02000111 RID: 273
	public static class EnumHelper<T> where T : struct, Enum
	{
		// Token: 0x060009DF RID: 2527 RVA: 0x0001C390 File Offset: 0x0001A590
		public static string GetName(T value)
		{
			return Enum.GetName(typeof(T), value);
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x0001C3A7 File Offset: 0x0001A5A7
		public static string[] GetNames()
		{
			return Enum.GetNames(typeof(T));
		}

		// Token: 0x060009E1 RID: 2529 RVA: 0x0001C3B8 File Offset: 0x0001A5B8
		public static Type GetUnderlyingType()
		{
			return Enum.GetUnderlyingType(typeof(T));
		}

		// Token: 0x060009E2 RID: 2530 RVA: 0x0001C3C9 File Offset: 0x0001A5C9
		public static T[] GetValues()
		{
			return Enumerable.ToArray<T>(Enumerable.Cast<T>(Enum.GetValues(typeof(T))));
		}

		// Token: 0x060009E3 RID: 2531 RVA: 0x0001C3E4 File Offset: 0x0001A5E4
		public static T Parse(string value)
		{
			return (T)((object)Enum.Parse(typeof(T), value));
		}

		// Token: 0x060009E4 RID: 2532 RVA: 0x0001C3FB File Offset: 0x0001A5FB
		public static T Parse(string value, bool ignoreCase)
		{
			return (T)((object)Enum.Parse(typeof(T), value, ignoreCase));
		}

		// Token: 0x060009E5 RID: 2533 RVA: 0x0001C413 File Offset: 0x0001A613
		public static T Max()
		{
			return Enumerable.Max<T>(EnumHelper<T>.GetValues());
		}

		// Token: 0x060009E6 RID: 2534 RVA: 0x0001C41F File Offset: 0x0001A61F
		public static T Min()
		{
			return Enumerable.Min<T>(EnumHelper<T>.GetValues());
		}
	}
}
