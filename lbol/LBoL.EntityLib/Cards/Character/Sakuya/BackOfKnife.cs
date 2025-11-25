using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class BackOfKnife : Card
	{
		[UsedImplicitly]
		public int IncreasedDamage
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Count<Card>(base.Battle.ExileZone, (Card c) => c is Knife) * base.Value1;
				}
				return 0;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			GroupOfKnife groupOfKnife = Library.CreateCard<GroupOfKnife>();
			List<Card> knifes = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.ExileZone, (Card card) => card is Knife));
			groupOfKnife.DeltaDamage = this.IncreasedDamage;
			yield return new AddCardsToHandAction(new Card[] { groupOfKnife });
			foreach (Card card2 in knifes)
			{
				yield return new RemoveCardAction(card2);
			}
			List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			yield break;
			yield break;
		}
	}
}
