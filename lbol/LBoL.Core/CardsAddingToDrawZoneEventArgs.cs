using System;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x02000026 RID: 38
	public class CardsAddingToDrawZoneEventArgs : GameEventArgs
	{
		// Token: 0x17000057 RID: 87
		// (get) Token: 0x0600012A RID: 298 RVA: 0x00004120 File Offset: 0x00002320
		// (set) Token: 0x0600012B RID: 299 RVA: 0x00004128 File Offset: 0x00002328
		public Card[] Cards { get; internal set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x0600012C RID: 300 RVA: 0x00004131 File Offset: 0x00002331
		// (set) Token: 0x0600012D RID: 301 RVA: 0x00004139 File Offset: 0x00002339
		public DrawZoneTarget DrawZoneTarget { get; internal set; }

		// Token: 0x0600012E RID: 302 RVA: 0x00004142 File Offset: 0x00002342
		protected override string GetBaseDebugString()
		{
			return string.Format("Cards = [{0}], -> {1}", string.Join(", ", Enumerable.Select<Card, string>(this.Cards, new Func<Card, string>(GameEventArgs.DebugString))), this.DrawZoneTarget);
		}
	}
}
