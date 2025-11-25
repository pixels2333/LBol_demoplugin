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
	[UsedImplicitly]
	public sealed class ExtraBlizzard : StatusEffect
	{
		[UsedImplicitly]
		public DamageInfo Damage
		{
			get
			{
				return DamageInfo.Attack((float)base.Level, true);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}
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
