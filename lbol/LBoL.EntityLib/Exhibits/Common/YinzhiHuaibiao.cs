using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class YinzhiHuaibiao : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn)
			{
				base.Counter = (base.Counter + 1) % base.Value1;
				if (base.Counter == 0)
				{
					base.NotifyActivating();
					yield return new GainManaAction(base.Mana);
				}
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
