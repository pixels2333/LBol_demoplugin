using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x0200008C RID: 140
	[UsedImplicitly]
	public sealed class AbsorbPower : StatusEffect
	{
		// Token: 0x06000203 RID: 515 RVA: 0x000063CA File Offset: 0x000045CA
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
		}

		// Token: 0x06000204 RID: 516 RVA: 0x000063E9 File Offset: 0x000045E9
		private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
