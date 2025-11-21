using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Others
{
	// Token: 0x02000039 RID: 57
	[UsedImplicitly]
	public sealed class CatchGoblinSe : StatusEffect
	{
		// Token: 0x060000A8 RID: 168 RVA: 0x000032C7 File Offset: 0x000014C7
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DieEventArgs>(base.Owner.Died, new EventSequencedReactor<DieEventArgs>(this.OnOwnerDied));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
			base.Highlight = true;
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00003305 File Offset: 0x00001505
		private IEnumerable<BattleAction> OnOwnerDied(DieEventArgs args)
		{
			yield return new GainMoneyAction(50, SpecialSourceType.None);
			yield break;
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00003310 File Offset: 0x00001510
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * 0.66f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
	}
}
