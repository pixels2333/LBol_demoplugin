using System;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x0200001D RID: 29
	public class CardEventArgs : GameEventArgs
	{
		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000F7 RID: 247 RVA: 0x00003E36 File Offset: 0x00002036
		// (set) Token: 0x060000F8 RID: 248 RVA: 0x00003E3E File Offset: 0x0000203E
		public Card Card { get; internal set; }

		// Token: 0x060000F9 RID: 249 RVA: 0x00003E47 File Offset: 0x00002047
		protected override string GetBaseDebugString()
		{
			return "Card = " + GameEventArgs.DebugString(this.Card);
		}
	}
}
