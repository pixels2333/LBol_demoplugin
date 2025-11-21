using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000035 RID: 53
	public class BlockShieldEventArgs : GameEventArgs
	{
		// Token: 0x17000085 RID: 133
		// (get) Token: 0x060001A1 RID: 417 RVA: 0x00004827 File Offset: 0x00002A27
		// (set) Token: 0x060001A2 RID: 418 RVA: 0x0000482F File Offset: 0x00002A2F
		public Unit Source { get; internal set; }

		// Token: 0x17000086 RID: 134
		// (get) Token: 0x060001A3 RID: 419 RVA: 0x00004838 File Offset: 0x00002A38
		// (set) Token: 0x060001A4 RID: 420 RVA: 0x00004840 File Offset: 0x00002A40
		public Unit Target { get; internal set; }

		// Token: 0x17000087 RID: 135
		// (get) Token: 0x060001A5 RID: 421 RVA: 0x00004849 File Offset: 0x00002A49
		// (set) Token: 0x060001A6 RID: 422 RVA: 0x00004851 File Offset: 0x00002A51
		public float Block { get; set; }

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x060001A7 RID: 423 RVA: 0x0000485A File Offset: 0x00002A5A
		// (set) Token: 0x060001A8 RID: 424 RVA: 0x00004862 File Offset: 0x00002A62
		public float Shield { get; set; }

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x060001A9 RID: 425 RVA: 0x0000486B File Offset: 0x00002A6B
		// (set) Token: 0x060001AA RID: 426 RVA: 0x00004873 File Offset: 0x00002A73
		public bool HasBlock { get; set; }

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x060001AB RID: 427 RVA: 0x0000487C File Offset: 0x00002A7C
		// (set) Token: 0x060001AC RID: 428 RVA: 0x00004884 File Offset: 0x00002A84
		public bool HasShield { get; set; }

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x060001AD RID: 429 RVA: 0x0000488D File Offset: 0x00002A8D
		// (set) Token: 0x060001AE RID: 430 RVA: 0x00004895 File Offset: 0x00002A95
		public BlockShieldType Type { get; set; }

		// Token: 0x060001AF RID: 431 RVA: 0x000048A0 File Offset: 0x00002AA0
		protected override string GetBaseDebugString()
		{
			if (this.Type != BlockShieldType.Unspecified)
			{
				return string.Format("{0} --- {{B: {1}, S: {2}, Type: {3}}} --> {4}", new object[]
				{
					GameEventArgs.DebugString(this.Source),
					this.Block,
					this.Shield,
					this.Type,
					GameEventArgs.DebugString(this.Target)
				});
			}
			return string.Format("{0} --- {{B: {1}, S: {2}}} --> {3}", new object[]
			{
				GameEventArgs.DebugString(this.Source),
				this.Block,
				this.Shield,
				GameEventArgs.DebugString(this.Target)
			});
		}
	}
}
