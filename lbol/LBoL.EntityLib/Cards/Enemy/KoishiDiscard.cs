using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x02000361 RID: 865
	public sealed class KoishiDiscard : Card
	{
		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000C77 RID: 3191 RVA: 0x000183B4 File Offset: 0x000165B4
		[UsedImplicitly]
		public string CardName
		{
			get
			{
				if (this.DiscardedCard == null)
				{
					return "Game.UnknownCard".Localize(true);
				}
				return this.DiscardedCard.Name;
			}
		}

		// Token: 0x17000164 RID: 356
		// (get) Token: 0x06000C78 RID: 3192 RVA: 0x000183D5 File Offset: 0x000165D5
		// (set) Token: 0x06000C79 RID: 3193 RVA: 0x000183DD File Offset: 0x000165DD
		public Card DiscardedCard { get; set; }

		// Token: 0x06000C7A RID: 3194 RVA: 0x000183E6 File Offset: 0x000165E6
		protected override string GetBaseDescription()
		{
			if (this.DiscardedCard == null)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000C7B RID: 3195 RVA: 0x000183FD File Offset: 0x000165FD
		public override IEnumerable<BattleAction> OnDraw()
		{
			if (Enumerable.Any<Card>(base.Battle.HandZone, (Card card) => card != this))
			{
				Card discard = Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this).Sample(base.BattleRng);
				if (discard != null)
				{
					this.DiscardedCard = discard;
					yield return new DiscardAction(discard);
					if (discard is KoishiDiscard)
					{
						yield return new ExileCardAction(discard);
					}
					if (base.Battle.DrawAfterDiscard > 0)
					{
						int drawAfterDiscard = base.Battle.DrawAfterDiscard;
						base.Battle.DrawAfterDiscard = 0;
						yield return new DrawManyCardAction(drawAfterDiscard);
					}
				}
				discard = null;
			}
			yield break;
		}
	}
}
