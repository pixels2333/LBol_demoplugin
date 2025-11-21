using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x0200001D RID: 29
	[UsedImplicitly]
	public sealed class PerfectServantWSe : StatusEffect
	{
		// Token: 0x06000040 RID: 64 RVA: 0x000026A6 File Offset: 0x000008A6
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x06000041 RID: 65 RVA: 0x000026C5 File Offset: 0x000008C5
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			if (base.Battle.HandZone.NotEmpty<Card>())
			{
				foreach (Card card in base.Battle.HandZone)
				{
					if (!(card.Cost == ManaGroup.Empty))
					{
						ManaColor[] array = card.Cost.EnumerateComponents().SampleManyOrAll(base.Level, base.GameRun.BattleRng);
						card.DecreaseTurnCost(ManaGroup.FromComponents(array));
					}
				}
			}
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
