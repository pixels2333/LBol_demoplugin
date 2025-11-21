using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000017 RID: 23
	[UsedImplicitly]
	public sealed class GainKnifeSe : StatusEffect
	{
		// Token: 0x06000031 RID: 49 RVA: 0x00002579 File Offset: 0x00000779
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x06000032 RID: 50 RVA: 0x00002598 File Offset: 0x00000798
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Level, false), AddCardsType.Normal);
			yield break;
		}
	}
}
