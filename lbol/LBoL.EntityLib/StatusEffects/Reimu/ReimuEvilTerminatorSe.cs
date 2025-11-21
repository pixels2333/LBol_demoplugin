using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x0200002F RID: 47
	[UsedImplicitly]
	public sealed class ReimuEvilTerminatorSe : StatusEffect
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000084 RID: 132 RVA: 0x00002E28 File Offset: 0x00001028
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Philosophies(base.Level);
			}
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00002E35 File Offset: 0x00001035
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00002E54 File Offset: 0x00001054
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs arg)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new GainManaAction(this.Mana);
			yield break;
		}
	}
}
