using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000173 RID: 371
	public sealed class DreamCardsAction : SimpleAction
	{
		// Token: 0x06000E38 RID: 3640 RVA: 0x0002714B File Offset: 0x0002534B
		public DreamCardsAction(int count, int playFollowCount = 0)
		{
			this._count = count;
			this._playFollowCount = playFollowCount;
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x0002716C File Offset: 0x0002536C
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x00027194 File Offset: 0x00025394
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			if (base.Battle.DrawZone.NotEmpty<Card>())
			{
				this._cards.AddRange(Enumerable.Take<Card>(base.Battle.DrawZone, this._count));
				foreach (Card card in this._cards)
				{
					if (this._playFollowCount > 0 && card.IsFollowCard)
					{
						yield return new PlayCardAction(card);
						this._playFollowCount--;
					}
					else
					{
						MoveCardAction moveCardAction = new MoveCardAction(card, CardZone.Discard)
						{
							DreamCardsAction = this
						};
						yield return moveCardAction;
						card.IsDreamCard = true;
					}
					card = null;
				}
				List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			}
			yield break;
			yield break;
		}

		// Token: 0x170004EF RID: 1263
		// (get) Token: 0x06000E3B RID: 3643 RVA: 0x000271A4 File Offset: 0x000253A4
		public IReadOnlyList<Card> Cards
		{
			get
			{
				return this._cards.AsReadOnly();
			}
		}

		// Token: 0x170004F0 RID: 1264
		// (get) Token: 0x06000E3C RID: 3644 RVA: 0x000271B1 File Offset: 0x000253B1
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0400066C RID: 1644
		private readonly int _count;

		// Token: 0x0400066D RID: 1645
		private int _playFollowCount;

		// Token: 0x0400066E RID: 1646
		private readonly List<Card> _cards = new List<Card>();
	}
}
