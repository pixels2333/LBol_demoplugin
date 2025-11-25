using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class MarisaR : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Attack && (args.ConsumingMana.Red > 0 || args.ConsumingMana.Philosophy > 0))
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Charging>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
