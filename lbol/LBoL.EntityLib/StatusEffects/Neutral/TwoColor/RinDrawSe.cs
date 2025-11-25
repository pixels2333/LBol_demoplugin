using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	public sealed class RinDrawSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardDrawn, new EventSequencedReactor<CardEventArgs>(this.OnCardDrawn));
		}
		private IEnumerable<BattleAction> OnCardDrawn(CardEventArgs args)
		{
			if (args.Card.CardType == CardType.Status)
			{
				base.NotifyActivating();
				yield return new DrawManyCardAction(base.Level);
			}
			yield break;
		}
	}
}
