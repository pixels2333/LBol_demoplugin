using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Enemy
{
	public sealed class Frog : Card
	{
		[UsedImplicitly]
		public string CardName
		{
			get
			{
				if (this.OriginalCard == null)
				{
					return "Game.UnknownCard".Localize(true);
				}
				return this.OriginalCard.Name;
			}
		}
		public Card OriginalCard { get; set; }
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card originalCard = this.OriginalCard;
			if (originalCard != null && originalCard.Battle == null)
			{
				yield return new AddCardsToHandAction(new Card[] { this.OriginalCard });
			}
			yield break;
		}
		public override IEnumerable<BattleAction> AfterUseAction()
		{
			yield return new RemoveCardAction(this);
			yield break;
		}
		public override IEnumerable<BattleAction> AfterFollowPlayAction()
		{
			yield return new RemoveCardAction(this);
			yield break;
		}
		public override IEnumerable<Card> EnumerateRelativeCards()
		{
			if (this.OriginalCard != null)
			{
				yield return this.OriginalCard;
			}
			yield break;
		}
	}
}
