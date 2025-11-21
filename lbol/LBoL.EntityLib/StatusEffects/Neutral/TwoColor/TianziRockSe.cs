using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x0200004B RID: 75
	[UsedImplicitly]
	public sealed class TianziRockSe : StatusEffect
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x060000EB RID: 235 RVA: 0x00003A48 File Offset: 0x00001C48
		protected override IEnumerable<string> RelativeEffects
		{
			get
			{
				if (base.Limit != 1)
				{
					return null;
				}
				return new string[] { "LockedOn" };
			}
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00003A63 File Offset: 0x00001C63
		protected override string GetBaseDescription()
		{
			if (base.Limit != 1)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00003A7B File Offset: 0x00001C7B
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(unit.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00003AB0 File Offset: 0x00001CB0
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = damageInfo.ReduceBy(base.Level);
				args.AddModifier(this);
			}
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00003AE8 File Offset: 0x00001CE8
		private IEnumerable<BattleAction> OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			if (base.Limit == 1 && args.Effect is LockedOn)
			{
				args.CancelBy(this);
				base.NotifyActivating();
				yield return PerformAction.Sfx("Amulet", 0f);
				yield return PerformAction.SePop(base.Owner, args.Effect.Name);
			}
			yield break;
		}
	}
}
