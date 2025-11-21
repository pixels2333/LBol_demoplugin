using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.StatusEffects.Marisa
{
	// Token: 0x02000067 RID: 103
	public sealed class LearnAstrologySe : StatusEffect
	{
		// Token: 0x06000163 RID: 355 RVA: 0x00004B1C File Offset: 0x00002D1C
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x06000164 RID: 356 RVA: 0x00004B3B File Offset: 0x00002D3B
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Astrology> list = Enumerable.ToList<Astrology>(Enumerable.OfType<Astrology>(base.Battle.HandZone));
			if (list.Count < base.Level)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<Astrology>(base.Level - list.Count, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
