using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.White
{
	[UsedImplicitly]
	public sealed class YukariSwarm : Card
	{
		private Guns Guns1
		{
			get
			{
				return new Guns(base.Config.GunName);
			}
		}
		private Guns Guns2
		{
			get
			{
				return new Guns(new string[]
				{
					base.Config.GunName,
					base.Config.GunNameBurst
				});
			}
		}
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.ExileZone.Count >= base.Value1;
			}
		}
		public override Interaction Precondition()
		{
			if (base.Value2 <= 0)
			{
				return null;
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, base.Value2, list);
			}
			return null;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectHandInteraction)precondition).SelectedCards;
				if (selectedCards.Count > 0)
				{
					yield return new ExileManyCardAction(selectedCards);
				}
			}
			base.CardGuns = ((base.Battle.ExileZone.Count >= base.Value1) ? this.Guns2 : this.Guns1);
			foreach (GunPair gunPair in base.CardGuns.GunPairs)
			{
				yield return base.AttackAction(selector, gunPair);
			}
			List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			yield break;
			yield break;
		}
	}
}
