using System;

namespace LBoL.Base
{
	// Token: 0x0200000D RID: 13
	public sealed class KeywordAttribute : Attribute
	{
		// Token: 0x04000033 RID: 51
		public const bool DefaultAutoAppend = true;

		// Token: 0x04000034 RID: 52
		public bool AutoAppend = true;

		// Token: 0x04000035 RID: 53
		public bool Hidden;

		// Token: 0x04000036 RID: 54
		public const bool DefaultIsVerbose = false;

		// Token: 0x04000037 RID: 55
		public bool IsVerbose;
	}
}
