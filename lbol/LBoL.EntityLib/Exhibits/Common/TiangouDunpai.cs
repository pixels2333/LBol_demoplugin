using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class TiangouDunpai : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				base.Active = true;
			});
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Active && args.Card.CardType == CardType.Defense)
			{
				base.NotifyActivating();
				base.Active = false;
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Active = false;
		}
	}
}
