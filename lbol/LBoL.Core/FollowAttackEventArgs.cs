using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x02000022 RID: 34
	public class FollowAttackEventArgs : GameEventArgs
	{
		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000112 RID: 274 RVA: 0x00003FD5 File Offset: 0x000021D5
		// (set) Token: 0x06000113 RID: 275 RVA: 0x00003FDD File Offset: 0x000021DD
		public UnitSelector SourceSelector { get; internal set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000114 RID: 276 RVA: 0x00003FE6 File Offset: 0x000021E6
		// (set) Token: 0x06000115 RID: 277 RVA: 0x00003FEE File Offset: 0x000021EE
		public int Count { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000116 RID: 278 RVA: 0x00003FF7 File Offset: 0x000021F7
		// (set) Token: 0x06000117 RID: 279 RVA: 0x00003FFF File Offset: 0x000021FF
		public bool RandomFiller { get; set; }

		// Token: 0x06000118 RID: 280 RVA: 0x00004008 File Offset: 0x00002208
		protected override string GetBaseDebugString()
		{
			return "FollowAttack: " + this.Count.ToString() + " -> " + GameEventArgs.DebugString(this.SourceSelector);
		}

		// Token: 0x040000AC RID: 172
		public readonly List<Card> Cards = new List<Card>();
	}
}
