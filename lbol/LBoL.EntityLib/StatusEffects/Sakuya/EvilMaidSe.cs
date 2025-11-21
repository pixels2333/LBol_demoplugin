using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x02000016 RID: 22
	[UsedImplicitly]
	public sealed class EvilMaidSe : StatusEffect
	{
		// Token: 0x0600002E RID: 46 RVA: 0x00002542 File Offset: 0x00000742
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarting));
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00002561 File Offset: 0x00000761
		private IEnumerable<BattleAction> OnOwnerTurnStarting(UnitEventArgs args)
		{
			base.NotifyActivating();
			Unit player = base.Battle.Player;
			int? num = new int?(base.Level);
			yield return new ApplyStatusEffectAction<EvilMaidDoubleAttack>(player, default(int?), num, default(int?), default(int?), 0f, true);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
