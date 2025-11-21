using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x02000036 RID: 54
	[UsedImplicitly]
	public sealed class SenluoJiejieSe : StatusEffect
	{
		// Token: 0x0600009F RID: 159 RVA: 0x0000321B File Offset: 0x0000141B
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x0000323A File Offset: 0x0000143A
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			if (base.Owner.Shield > 0)
			{
				base.NotifyActivating();
				int shield = base.Owner.Shield;
				yield return new LoseBlockShieldAction(base.Owner, 0, shield, false);
				yield return new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)shield, false), "森罗", GunType.Single);
			}
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
