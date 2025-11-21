using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.White
{
	// Token: 0x0200003D RID: 61
	public sealed class MoonWorldSe : StatusEffect
	{
		// Token: 0x060000BA RID: 186 RVA: 0x00003470 File Offset: 0x00001670
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new GameEventHandler<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00003494 File Offset: 0x00001694
		private void OnTurnStarted(UnitEventArgs args)
		{
			int i = 0;
			bool flag = false;
			while (i < base.Level)
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => !card.IsPurified && card.Cost.HasTrivialOrHybrid));
				if (list.Count > 0)
				{
					Card card3 = list.Sample(base.GameRun.BattleRng);
					card3.NotifyActivating();
					card3.IsPurified = true;
					if (!flag)
					{
						base.NotifyActivating();
						flag = true;
					}
				}
				else
				{
					List<Card> list2 = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => !card.IsPurified));
					if (list2.Count <= 0)
					{
						break;
					}
					Card card2 = list2.Sample(base.GameRun.BattleRng);
					card2.NotifyActivating();
					card2.IsPurified = true;
					if (!flag)
					{
						base.NotifyActivating();
						flag = true;
					}
				}
				i++;
			}
		}
	}
}
