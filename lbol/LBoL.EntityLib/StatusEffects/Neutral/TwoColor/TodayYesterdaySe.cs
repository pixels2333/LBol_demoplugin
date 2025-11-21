using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x0200004C RID: 76
	[UsedImplicitly]
	public sealed class TodayYesterdaySe : StatusEffect
	{
		// Token: 0x060000F1 RID: 241 RVA: 0x00003B07 File Offset: 0x00001D07
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived));
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00003B2B File Offset: 0x00001D2B
		private IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.IsGrazed)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
