using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class ColdSnap : Card
	{
		[UsedImplicitly]
		public int BlueCount
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return Enumerable.Count<Card>(base.Battle.DrawZone, (Card card) => Enumerable.Contains<ManaColor>(card.Config.Colors, ManaColor.Blue));
			}
		}
		protected override string GetBaseDescription()
		{
			if (base.Battle == null || this.BlueCount >= base.Value1)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.DrawZone.Count <= 0 || base.Battle.HandIsFull)
			{
				yield break;
			}
			List<Card> cards = Enumerable.ToList<Card>(Enumerable.Take<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => Enumerable.Contains<ManaColor>(card.Config.Colors, ManaColor.Blue)), base.Value1));
			if (cards.Count > 0)
			{
				foreach (Card card3 in cards)
				{
					if (base.Battle.HandIsFull)
					{
						break;
					}
					yield return new MoveCardAction(card3, CardZone.Hand);
				}
				List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(cards, (Card card) => card.Zone == CardZone.Hand));
				if (list.Count > 0)
				{
					foreach (Card card2 in list)
					{
						card2.NotifyActivating();
					}
					yield return base.DefenseAction(0, base.Value2 * list.Count, BlockShieldType.Direct, false);
				}
			}
			yield break;
			yield break;
		}
	}
}
