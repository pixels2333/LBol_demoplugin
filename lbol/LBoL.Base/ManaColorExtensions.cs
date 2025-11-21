using System;

namespace LBoL.Base
{
	// Token: 0x02000011 RID: 17
	public static class ManaColorExtensions
	{
		// Token: 0x0600002C RID: 44 RVA: 0x000029F3 File Offset: 0x00000BF3
		public static char ToShortName(this ManaColor color)
		{
			return ManaColors.GetShortName(color);
		}

		// Token: 0x0600002D RID: 45 RVA: 0x000029FB File Offset: 0x00000BFB
		public static string ToLongName(this ManaColor color)
		{
			return ManaColors.GetLongName(color);
		}
	}
}
