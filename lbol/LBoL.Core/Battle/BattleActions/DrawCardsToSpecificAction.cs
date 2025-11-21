using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000170 RID: 368
	public class DrawCardsToSpecificAction : SimpleAction
	{
		// Token: 0x06000E2A RID: 3626 RVA: 0x00027049 File Offset: 0x00025249
		public DrawCardsToSpecificAction(int count = 1)
		{
			this._count = count;
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x00027064 File Offset: 0x00025264
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x0002708C File Offset: 0x0002528C
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			int num = this._count - base.Battle.HandZone.Count;
			if (num > 0)
			{
				yield return new DrawManyCardAction(num);
			}
			yield break;
		}

		// Token: 0x170004EA RID: 1258
		// (get) Token: 0x06000E2D RID: 3629 RVA: 0x0002709C File Offset: 0x0002529C
		public IReadOnlyList<Card> DrawnCards
		{
			get
			{
				return this._cards.AsReadOnly();
			}
		}

		// Token: 0x170004EB RID: 1259
		// (get) Token: 0x06000E2E RID: 3630 RVA: 0x000270A9 File Offset: 0x000252A9
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04000667 RID: 1639
		private readonly int _count;

		// Token: 0x04000668 RID: 1640
		private readonly List<Card> _cards = new List<Card>();
	}
}
