using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Cirno
{
	[UsedImplicitly]
	public sealed class FairyTreeSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			Card card = args.Card;
			if (card.CardType == CardType.Friend && card.Summoning)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Battle.Player, base.Battle.Player, base.Level, 0, BlockShieldType.Direct, true);
			}
			yield break;
		}
	}
}
