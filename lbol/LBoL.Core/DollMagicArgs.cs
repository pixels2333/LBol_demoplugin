using System;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x02000039 RID: 57
	public class DollMagicArgs : GameEventArgs
	{
		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060001BF RID: 447 RVA: 0x00004A05 File Offset: 0x00002C05
		// (set) Token: 0x060001C0 RID: 448 RVA: 0x00004A0D File Offset: 0x00002C0D
		public Doll Doll { get; set; }

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060001C1 RID: 449 RVA: 0x00004A16 File Offset: 0x00002C16
		// (set) Token: 0x060001C2 RID: 450 RVA: 0x00004A1E File Offset: 0x00002C1E
		public int Magic { get; set; }

		// Token: 0x060001C3 RID: 451 RVA: 0x00004A27 File Offset: 0x00002C27
		protected override string GetBaseDebugString()
		{
			return string.Format("Doll = {0}, Magic = {1}", GameEventArgs.DebugString(this.Doll), this.Magic);
		}
	}
}
