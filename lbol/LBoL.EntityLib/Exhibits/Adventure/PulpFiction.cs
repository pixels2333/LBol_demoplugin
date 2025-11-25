using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class PulpFiction : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.Counter = base.Value2;
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Counter > 0)
			{
				int num = base.Counter - 1;
				base.Counter = num;
				if (base.Battle.DrawZone.Count > 0)
				{
					base.NotifyActivating();
					for (int i = 0; i < base.Value1; i = num + 1)
					{
						Card card = Enumerable.LastOrDefault<Card>(base.Battle.DrawZone);
						if (card != null)
						{
							yield return new MoveCardAction(card, CardZone.Hand);
						}
						num = i;
					}
				}
				if (base.Counter == 1)
				{
					base.Blackout = true;
				}
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			if (base.Counter > 0)
			{
				base.Counter = 0;
			}
			base.Blackout = false;
		}
	}
}
