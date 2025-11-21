using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.ExtraTurn.Partners
{
	// Token: 0x0200008A RID: 138
	[UsedImplicitly]
	public sealed class LunaClockSe : ExtraTurnPartner
	{
		// Token: 0x060001F9 RID: 505 RVA: 0x000061C4 File Offset: 0x000043C4
		protected override void OnAdded(Unit unit)
		{
			base.ThisTurnActivating = false;
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				if (base.Battle.Player.IsExtraTurn && !base.Battle.Player.IsSuperExtraTurn && base.Battle.Player.GetStatusEffectExtend<ExtraTurnPartner>() == this)
				{
					base.ThisTurnActivating = true;
				}
			});
			base.HandleOwnerEvent<DamageDealingEventArgs>(base.Owner.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
			base.Highlight = true;
		}

		// Token: 0x060001FA RID: 506 RVA: 0x0000623C File Offset: 0x0000443C
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (base.ThisTurnActivating && args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.MultiplyBy(0);
				args.AddModifier(this);
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					base.NotifyActivating();
				}
			}
		}

		// Token: 0x060001FB RID: 507 RVA: 0x0000628E File Offset: 0x0000448E
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			if (base.ThisTurnActivating)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
