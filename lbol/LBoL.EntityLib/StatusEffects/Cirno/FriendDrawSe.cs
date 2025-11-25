using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Cirno
{
	[UsedImplicitly]
	public sealed class FriendDrawSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			Card card = args.Card;
			if (card.CardType == CardType.Friend && !card.Summoning)
			{
				base.NotifyActivating();
				yield return new DrawManyCardAction(base.Level);
				if (card.UltimateUsed)
				{
					ManaGroup manaGroup = ManaGroup.Empty;
					for (int i = 0; i < base.Level; i++)
					{
						manaGroup += ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
					}
					yield return new GainManaAction(manaGroup);
				}
			}
			yield break;
		}
	}
}
