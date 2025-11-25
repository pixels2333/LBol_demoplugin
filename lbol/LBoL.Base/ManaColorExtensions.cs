using System;
namespace LBoL.Base
{
	public static class ManaColorExtensions
	{
		public static char ToShortName(this ManaColor color)
		{
			return ManaColors.GetShortName(color);
		}
		public static string ToLongName(this ManaColor color)
		{
			return ManaColors.GetLongName(color);
		}
	}
}
