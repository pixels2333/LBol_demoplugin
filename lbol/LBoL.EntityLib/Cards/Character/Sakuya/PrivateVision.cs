using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Basic;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class PrivateVision : Card
	{
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (list.Count == 1)
			{
				this.oneTargetHand = list[0];
			}
			if (list.Count <= 1)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new DiscardAction(card);
				}
			}
			else if (this.oneTargetHand != null)
			{
				yield return new DiscardAction(this.oneTargetHand);
				this.oneTargetHand = null;
			}
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Value2 > 0)
			{
				yield return base.BuffAction<Reflect>(base.Value2, 0, 0, 0, 0.2f);
				Reflect statusEffect = base.Battle.Player.GetStatusEffect<Reflect>();
				if (statusEffect != null)
				{
					statusEffect.Gun = base.Config.GunName;
				}
			}
			yield break;
		}
		private Card oneTargetHand;
	}
}
