using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000171 RID: 369
	public class DrawManyCardAction : SimpleAction
	{
		// Token: 0x06000E2F RID: 3631 RVA: 0x000270AC File Offset: 0x000252AC
		public DrawManyCardAction(int count)
		{
			this._count = count;
		}

		// Token: 0x06000E30 RID: 3632 RVA: 0x000270C8 File Offset: 0x000252C8
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}

		// Token: 0x06000E31 RID: 3633 RVA: 0x000270F0 File Offset: 0x000252F0
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			int num;
			for (int i = 0; i < this._count; i = num)
			{
				DrawCardAction draw = new DrawCardAction();
				yield return draw;
				if (draw.Args.Card != null)
				{
					this._cards.Add(draw.Args.Card);
				}
				draw = null;
				num = i + 1;
			}
			yield break;
		}

		// Token: 0x170004EC RID: 1260
		// (get) Token: 0x06000E32 RID: 3634 RVA: 0x00027100 File Offset: 0x00025300
		public IReadOnlyList<Card> DrawnCards
		{
			get
			{
				return this._cards.AsReadOnly();
			}
		}

		// Token: 0x170004ED RID: 1261
		// (get) Token: 0x06000E33 RID: 3635 RVA: 0x0002710D File Offset: 0x0002530D
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04000669 RID: 1641
		private readonly int _count;

		// Token: 0x0400066A RID: 1642
		private readonly List<Card> _cards = new List<Card>();
	}
}
