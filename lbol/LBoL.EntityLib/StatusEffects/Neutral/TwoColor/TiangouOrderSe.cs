using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x0200004A RID: 74
	public sealed class TiangouOrderSe : StatusEffect
	{
		// Token: 0x060000E8 RID: 232 RVA: 0x00003A0A File Offset: 0x00001C0A
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x00003A29 File Offset: 0x00001C29
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f)
			{
				yield return new CastBlockShieldAction(base.Battle.Player, base.Battle.Player, base.Level, 0, BlockShieldType.Direct, false);
			}
			yield break;
		}
	}
}
