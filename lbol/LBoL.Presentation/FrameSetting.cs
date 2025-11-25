using System;
namespace LBoL.Presentation
{
	public struct FrameSetting
	{
		public FrameSetting(int vsyncCount, int frameRate)
		{
			this.VsyncCount = vsyncCount;
			this.FrameRate = frameRate;
		}
		public static bool Equals(FrameSetting left, FrameSetting right)
		{
			return (left.IsVsync && right.IsVsync && left.VsyncCount == right.VsyncCount) || (!left.IsVsync && !right.IsVsync && left.FrameRate == right.FrameRate);
		}
		public bool Equals(FrameSetting other)
		{
			return FrameSetting.Equals(this, other);
		}
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
		public bool IsVsync
		{
			get
			{
				return this.VsyncCount > 0;
			}
		}
		public int VsyncCount;
		public int FrameRate;
	}
}
