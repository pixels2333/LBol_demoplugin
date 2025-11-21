using System;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x02000023 RID: 35
	public class CardMovingEventArgs : GameEventArgs
	{
		// Token: 0x17000051 RID: 81
		// (get) Token: 0x0600011A RID: 282 RVA: 0x00004050 File Offset: 0x00002250
		// (set) Token: 0x0600011B RID: 283 RVA: 0x00004058 File Offset: 0x00002258
		public Card Card { get; internal set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x0600011C RID: 284 RVA: 0x00004061 File Offset: 0x00002261
		// (set) Token: 0x0600011D RID: 285 RVA: 0x00004069 File Offset: 0x00002269
		public CardZone SourceZone { get; internal set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x0600011E RID: 286 RVA: 0x00004072 File Offset: 0x00002272
		// (set) Token: 0x0600011F RID: 287 RVA: 0x0000407A File Offset: 0x0000227A
		public CardZone DestinationZone { get; internal set; }

		// Token: 0x06000120 RID: 288 RVA: 0x00004083 File Offset: 0x00002283
		protected override string GetBaseDebugString()
		{
			return string.Format("Card = {0}, {1} -> {2}", GameEventArgs.DebugString(this.Card), this.SourceZone, this.DestinationZone);
		}
	}
}
