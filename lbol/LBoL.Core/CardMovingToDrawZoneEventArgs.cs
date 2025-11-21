using System;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x02000025 RID: 37
	public class CardMovingToDrawZoneEventArgs : GameEventArgs
	{
		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000122 RID: 290 RVA: 0x000040B8 File Offset: 0x000022B8
		// (set) Token: 0x06000123 RID: 291 RVA: 0x000040C0 File Offset: 0x000022C0
		public Card Card { get; internal set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000124 RID: 292 RVA: 0x000040C9 File Offset: 0x000022C9
		// (set) Token: 0x06000125 RID: 293 RVA: 0x000040D1 File Offset: 0x000022D1
		public CardZone SourceZone { get; internal set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x06000126 RID: 294 RVA: 0x000040DA File Offset: 0x000022DA
		// (set) Token: 0x06000127 RID: 295 RVA: 0x000040E2 File Offset: 0x000022E2
		public DrawZoneTarget DrawZoneTarget { get; internal set; }

		// Token: 0x06000128 RID: 296 RVA: 0x000040EB File Offset: 0x000022EB
		protected override string GetBaseDebugString()
		{
			return string.Format("Card = {0}, {1} -> {2}", GameEventArgs.DebugString(this.Card), this.SourceZone, this.DrawZoneTarget);
		}
	}
}
