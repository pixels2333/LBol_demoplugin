using System;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;

namespace LBoL.Core
{
	// Token: 0x02000020 RID: 32
	public class CardsEventArgs : GameEventArgs
	{
		// Token: 0x17000048 RID: 72
		// (get) Token: 0x06000101 RID: 257 RVA: 0x00003EB7 File Offset: 0x000020B7
		// (set) Token: 0x06000102 RID: 258 RVA: 0x00003EBF File Offset: 0x000020BF
		public Card[] Cards { get; set; }

		// Token: 0x06000103 RID: 259 RVA: 0x00003EC8 File Offset: 0x000020C8
		protected override string GetBaseDebugString()
		{
			return "Cards = [" + ", ".Join(Enumerable.Select<Card, string>(this.Cards, new Func<Card, string>(GameEventArgs.DebugString))) + "]";
		}
	}
}
