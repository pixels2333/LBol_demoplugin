using System;

namespace LBoL.Core
{
	// Token: 0x02000018 RID: 24
	public enum CancelCause
	{
		// Token: 0x04000086 RID: 134
		None,
		// Token: 0x04000087 RID: 135
		Failure,
		// Token: 0x04000088 RID: 136
		InvalidTarget,
		// Token: 0x04000089 RID: 137
		HandFull,
		// Token: 0x0400008A RID: 138
		ZoneFull,
		// Token: 0x0400008B RID: 139
		EmptyDraw,
		// Token: 0x0400008C RID: 140
		AlreadyExiled,
		// Token: 0x0400008D RID: 141
		DollSlotFull,
		// Token: 0x0400008E RID: 142
		DollSlotEmpty,
		// Token: 0x0400008F RID: 143
		UserCanceled = 10,
		// Token: 0x04000090 RID: 144
		Reaction = 4096
	}
}
