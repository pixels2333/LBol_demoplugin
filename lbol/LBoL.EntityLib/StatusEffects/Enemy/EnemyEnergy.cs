using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x0200009C RID: 156
	[UsedImplicitly]
	public sealed class EnemyEnergy : StatusEffect, IOpposing<EnemyEnergyNegative>
	{
		// Token: 0x06000230 RID: 560 RVA: 0x0000682D File Offset: 0x00004A2D
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
		}

		// Token: 0x06000231 RID: 561 RVA: 0x0000684C File Offset: 0x00004A4C
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			int num = args.DamageInfo.Damage.RoundToInt();
			if (num > 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<EnemyEnergy>(base.Owner, new int?(num), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x06000232 RID: 562 RVA: 0x00006863 File Offset: 0x00004A63
		public OpposeResult Oppose(EnemyEnergyNegative other)
		{
			base.Level = Math.Max(0, base.Level - other.Level);
			return OpposeResult.KeepSelf;
		}
	}
}
