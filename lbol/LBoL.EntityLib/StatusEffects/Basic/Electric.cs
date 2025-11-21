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
	// Token: 0x020000EF RID: 239
	[UsedImplicitly]
	public sealed class Electric : StatusEffect
	{
		// Token: 0x06000358 RID: 856 RVA: 0x00008C78 File Offset: 0x00006E78
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
		}

		// Token: 0x06000359 RID: 857 RVA: 0x00008C97 File Offset: 0x00006E97
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.Source != base.Owner && args.Source.IsAlive)
			{
				DamageInfo damageInfo = args.DamageInfo;
				if (damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f)
				{
					base.NotifyActivating();
					yield return new DamageAction(base.Owner, args.Source, DamageInfo.Reaction((float)base.Level, false), "电击", GunType.Single);
				}
			}
			yield break;
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x0600035A RID: 858 RVA: 0x00008CAE File Offset: 0x00006EAE
		public override string UnitEffectName
		{
			get
			{
				return "ElectricLoop";
			}
		}
	}
}
