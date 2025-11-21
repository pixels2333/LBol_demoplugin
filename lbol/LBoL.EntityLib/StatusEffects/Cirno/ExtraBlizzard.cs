using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000D9 RID: 217
	[UsedImplicitly]
	public sealed class ExtraBlizzard : StatusEffect
	{
		// Token: 0x1700005B RID: 91
		// (get) Token: 0x0600030A RID: 778 RVA: 0x00008361 File Offset: 0x00006561
		[UsedImplicitly]
		public DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)base.Level, true);
			}
		}

		// Token: 0x0600030B RID: 779 RVA: 0x00008370 File Offset: 0x00006570
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600030C RID: 780 RVA: 0x0000838F File Offset: 0x0000658F
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new DamageAction(base.Owner, base.Battle.AllAliveEnemies, this.Damage, "NoAni延长的冬日", GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (EnemyUnit enemyUnit in base.Battle.AllAliveEnemies)
			{
				yield return new ApplyStatusEffectAction<Cold>(enemyUnit, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
			yield break;
		}
	}
}
