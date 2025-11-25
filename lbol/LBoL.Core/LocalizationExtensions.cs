using System;
using System.Collections.Generic;
namespace LBoL.Core
{
	public static class LocalizationExtensions
	{
		public static string Localize(this string key, bool decorate = true)
		{
			return Localization.Localize(key, decorate);
		}
		public static string LocalizeFormat(this string key, params object[] args)
		{
			return Localization.LocalizeFormat(key, args);
		}
		public static IList<string> LocalizeStrings(this string key, bool decorate = true)
		{
			return Localization.LocalizeStrings(key, decorate);
		}
	}
}
