using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Reimu;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x02000037 RID: 55
	[UsedImplicitly]
	public sealed class TaijiLiangyiSe : StatusEffect
	{
		// Token: 0x060000A2 RID: 162 RVA: 0x00003252 File Offset: 0x00001452
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x00003271 File Offset: 0x00001471
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<YinyangCard> list = Enumerable.ToList<YinyangCard>(Enumerable.OfType<YinyangCard>(base.Battle.HandZone));
			if (list.Count < base.Level)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<YinyangCard>(base.Level - list.Count, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
