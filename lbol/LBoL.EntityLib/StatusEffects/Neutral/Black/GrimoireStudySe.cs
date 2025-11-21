using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.StatusEffects.Neutral.Black
{
	// Token: 0x02000061 RID: 97
	public sealed class GrimoireStudySe : StatusEffect
	{
		// Token: 0x06000155 RID: 341 RVA: 0x00004A18 File Offset: 0x00002C18
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnding));
		}

		// Token: 0x06000156 RID: 342 RVA: 0x00004A37 File Offset: 0x00002C37
		private IEnumerable<BattleAction> OnOwnerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new AddCardsToHandAction(Library.CreateCards<PManaCard>(1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
