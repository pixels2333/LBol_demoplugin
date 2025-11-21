using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000139 RID: 313
	[UsedImplicitly]
	public sealed class ReimuR : ShiningExhibit
	{
		// Token: 0x0600044B RID: 1099 RVA: 0x0000B796 File Offset: 0x00009996
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x0000B7B5 File Offset: 0x000099B5
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
		{
			base.NotifyActivating();
			if (!base.Battle.BattleShouldEnd)
			{
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield return new HealAction(base.Owner, base.Owner, base.Value2, HealType.Normal, 0.2f);
			yield break;
		}
	}
}
