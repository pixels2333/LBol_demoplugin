using System;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x0200001E RID: 30
	public class CardTransformEventArgs : GameEventArgs
	{
		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000FB RID: 251 RVA: 0x00003E66 File Offset: 0x00002066
		// (set) Token: 0x060000FC RID: 252 RVA: 0x00003E6E File Offset: 0x0000206E
		public Card SourceCard { get; internal set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000FD RID: 253 RVA: 0x00003E77 File Offset: 0x00002077
		// (set) Token: 0x060000FE RID: 254 RVA: 0x00003E7F File Offset: 0x0000207F
		public Card DestinationCard { get; internal set; }

		// Token: 0x060000FF RID: 255 RVA: 0x00003E88 File Offset: 0x00002088
		protected override string GetBaseDebugString()
		{
			return "CardTransform: " + GameEventArgs.DebugString(this.SourceCard) + " -> " + GameEventArgs.DebugString(this.DestinationCard);
		}
	}
}
