using System;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.Core
{
	// Token: 0x0200002A RID: 42
	public class MoodChangeEventArgs : GameEventArgs
	{
		// Token: 0x17000062 RID: 98
		// (get) Token: 0x06000148 RID: 328 RVA: 0x000042A3 File Offset: 0x000024A3
		// (set) Token: 0x06000149 RID: 329 RVA: 0x000042AB File Offset: 0x000024AB
		public Unit Unit { get; internal set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600014A RID: 330 RVA: 0x000042B4 File Offset: 0x000024B4
		// (set) Token: 0x0600014B RID: 331 RVA: 0x000042BC File Offset: 0x000024BC
		public Mood BeforeMood { get; internal set; }

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x0600014C RID: 332 RVA: 0x000042C5 File Offset: 0x000024C5
		// (set) Token: 0x0600014D RID: 333 RVA: 0x000042CD File Offset: 0x000024CD
		public Mood AfterMood { get; internal set; }

		// Token: 0x0600014E RID: 334 RVA: 0x000042D8 File Offset: 0x000024D8
		protected override string GetBaseDebugString()
		{
			return string.Concat(new string[]
			{
				GameEventArgs.DebugString(this.Unit),
				": ",
				GameEventArgs.DebugString(this.BeforeMood),
				" -> ",
				GameEventArgs.DebugString(this.AfterMood)
			});
		}
	}
}
