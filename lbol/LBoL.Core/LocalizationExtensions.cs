using System;
using System.Collections.Generic;

namespace LBoL.Core
{
	// Token: 0x0200005C RID: 92
	public static class LocalizationExtensions
	{
		// Token: 0x0600041B RID: 1051 RVA: 0x0000E6DC File Offset: 0x0000C8DC
		public static string Localize(this string key, bool decorate = true)
		{
			return Localization.Localize(key, decorate);
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x0000E6E5 File Offset: 0x0000C8E5
		public static string LocalizeFormat(this string key, params object[] args)
		{
			return Localization.LocalizeFormat(key, args);
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x0000E6EE File Offset: 0x0000C8EE
		public static IList<string> LocalizeStrings(this string key, bool decorate = true)
		{
			return Localization.LocalizeStrings(key, decorate);
		}
	}
}
