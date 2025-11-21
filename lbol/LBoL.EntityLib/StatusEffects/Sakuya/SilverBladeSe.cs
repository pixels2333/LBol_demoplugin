using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000020 RID: 32
	[UsedImplicitly]
	public sealed class SilverBladeSe : StatusEffect
	{
		// Token: 0x0600004B RID: 75 RVA: 0x000027A3 File Offset: 0x000009A3
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnOwnerDamageDealt));
		}

		// Token: 0x0600004C RID: 76 RVA: 0x000027C2 File Offset: 0x000009C2
		private IEnumerable<BattleAction> OnOwnerDamageDealt(StatisticalDamageEventArgs args)
		{
			if (args.Cause == ActionCause.Card)
			{
				Card card = args.ActionSource as Card;
				if (card != null && card.CardType == CardType.Attack)
				{
					base.NotifyActivating();
					yield return new CastBlockShieldAction(base.Battle.Player, new ShieldInfo(base.Level, BlockShieldType.Direct), false);
				}
			}
			yield break;
		}
	}
}
