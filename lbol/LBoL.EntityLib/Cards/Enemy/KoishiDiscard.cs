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
	public sealed class KoishiDiscard : Card
	{
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
		public Card DiscardedCard { get; set; }
		protected override string GetBaseDescription()
		{
			if (this.DiscardedCard == null)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
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
