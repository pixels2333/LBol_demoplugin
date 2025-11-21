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
	// Token: 0x0200008D RID: 141
	[UsedImplicitly]
	public sealed class AbsorbSpirit : StatusEffect
	{
		// Token: 0x06000206 RID: 518 RVA: 0x00006401 File Offset: 0x00004601
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
		}

		// Token: 0x06000207 RID: 519 RVA: 0x00006420 File Offset: 0x00004620
		private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Spirit>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
