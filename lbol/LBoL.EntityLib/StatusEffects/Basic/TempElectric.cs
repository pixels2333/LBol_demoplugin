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

namespace LBoL.EntityLib.StatusEffects.Basic
{
	// Token: 0x020000F6 RID: 246
	[UsedImplicitly]
	public sealed class TempElectric : StatusEffect
	{
		// Token: 0x06000371 RID: 881 RVA: 0x00008ECC File Offset: 0x000070CC
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x06000372 RID: 882 RVA: 0x00008F08 File Offset: 0x00007108
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.Source != base.Owner && args.Source.IsAlive && args.DamageInfo.DamageType == DamageType.Attack && args.DamageInfo.Amount > 0f)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Owner, args.Source, DamageInfo.Reaction((float)base.Level, false), "电击", GunType.Single);
			}
			yield break;
		}

		// Token: 0x06000373 RID: 883 RVA: 0x00008F1F File Offset: 0x0000711F
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (!base.Owner.IsExtraTurn)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x06000374 RID: 884 RVA: 0x00008F2F File Offset: 0x0000712F
		public override string UnitEffectName
		{
			get
			{
				return "ElectricLoop";
			}
		}
	}
}
