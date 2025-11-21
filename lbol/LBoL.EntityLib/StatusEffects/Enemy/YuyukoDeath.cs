using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000CB RID: 203
	[UsedImplicitly]
	public sealed class YuyukoDeath : StatusEffect
	{
		// Token: 0x060002BF RID: 703 RVA: 0x000077D0 File Offset: 0x000059D0
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
			base.HandleOwnerEvent<HealEventArgs>(base.Owner.HealingReceiving, new GameEventHandler<HealEventArgs>(this.OnOwnerHealing));
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x0000780C File Offset: 0x00005A0C
		private IEnumerable<BattleAction> OnOwnerTurnEnded(GameEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd)
			{
				base.NotifyActivating();
				yield return PerformAction.Effect(base.Owner, "YuyukoDeathHit", 0f, "YuyukoDeathHit", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new DamageAction(base.Owner, base.Owner, DamageInfo.HpLose((float)base.Level, false), "Instant", GunType.Single);
			}
			yield break;
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x0000781C File Offset: 0x00005A1C
		private void OnOwnerHealing(HealEventArgs args)
		{
			base.NotifyActivating();
			args.CancelBy(this);
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060002C2 RID: 706 RVA: 0x0000782B File Offset: 0x00005A2B
		public override string UnitEffectName
		{
			get
			{
				return "YuyukoDeath";
			}
		}
	}
}
