using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Others
{
	// Token: 0x0200003B RID: 59
	public sealed class Poison : StatusEffect
	{
		// Token: 0x060000B1 RID: 177 RVA: 0x00003394 File Offset: 0x00001594
		protected override void OnAdded(Unit unit)
		{
			if (unit is EnemyUnit)
			{
				base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnStarted, new EventSequencedReactor<GameEventArgs>(this.OnAllEnemyTurnStarted));
				base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnEnemyTurnStarted));
				return;
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x00003401 File Offset: 0x00001601
		private IEnumerable<BattleAction> OnAllEnemyTurnStarted(GameEventArgs args)
		{
			return this.TakeEffect();
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x00003409 File Offset: 0x00001609
		private IEnumerable<BattleAction> OnEnemyTurnStarted(UnitEventArgs args)
		{
			Unit owner = base.Owner;
			if (owner == null || !owner.IsExtraTurn)
			{
				yield break;
			}
			foreach (BattleAction battleAction in this.TakeEffect())
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x00003419 File Offset: 0x00001619
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			return this.TakeEffect();
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00003421 File Offset: 0x00001621
		public IEnumerable<BattleAction> TakeEffect()
		{
			if (base.Owner == null || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return DamageAction.LoseLife(base.Owner, base.Level, "Poison");
			int num = base.Level - 1;
			base.Level = num;
			if (base.Level == 0)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
	}
}
