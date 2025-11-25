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
	public sealed class GuizuJiubei : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarting));
		}
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Attack)
			{
				int num = base.Counter + 1;
				base.Counter = num;
			}
		}
		private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
		{
			if (base.Counter > 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(base.Counter), default(int?), default(int?), default(int?), 0f, true);
				base.Counter = 0;
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
