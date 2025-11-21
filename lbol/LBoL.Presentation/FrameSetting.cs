using System;

namespace LBoL.Presentation
{
	// Token: 0x02000010 RID: 16
	public struct FrameSetting
	{
		// Token: 0x0600016B RID: 363 RVA: 0x000079B6 File Offset: 0x00005BB6
		public FrameSetting(int vsyncCount, int frameRate)
		{
			this.VsyncCount = vsyncCount;
			this.FrameRate = frameRate;
		}

		// Token: 0x0600016C RID: 364 RVA: 0x000079C8 File Offset: 0x00005BC8
		public static bool Equals(FrameSetting left, FrameSetting right)
		{
			return (left.IsVsync && right.IsVsync && left.VsyncCount == right.VsyncCount) || (!left.IsVsync && !right.IsVsync && left.FrameRate == right.FrameRate);
		}

		// Token: 0x0600016D RID: 365 RVA: 0x00007A1A File Offset: 0x00005C1A
		public bool Equals(FrameSetting other)
		{
			return FrameSetting.Equals(this, other);
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600016E RID: 366 RVA: 0x00007A28 File Offset: 0x00005C28
		public int FrameRateForSetting
		{
			get
			{
				if (!this.IsVsync)
				{
					return this.FrameRate;
				}
				return -1;
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600016F RID: 367 RVA: 0x00007A3A File Offset: 0x00005C3A
		public bool IsVsync
		{
			get
			{
				return this.VsyncCount > 0;
			}
		}

		// Token: 0x04000066 RID: 102
		public int VsyncCount;

		// Token: 0x04000067 RID: 103
		public int FrameRate;
	}
}
