using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class HeiseBijiben : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			List<EnemyUnit> realNameEnemies = new List<EnemyUnit>();
			foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup)
			{
				if (enemyUnit.IsAlive && enemyUnit.Config.RealName)
				{
					realNameEnemies.Add(enemyUnit);
				}
			}
			if (realNameEnemies.Count > 0)
			{
				yield return new DamageAction(base.Owner, realNameEnemies, DamageInfo.HpLose((float)base.Value1, false), "Instant", GunType.Single);
				List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(realNameEnemies, (EnemyUnit enemy) => enemy.IsAlive));
				foreach (EnemyUnit enemyUnit2 in list)
				{
					Unit unit = enemyUnit2;
					int? num = new int?(base.Value2);
					yield return new ApplyStatusEffectAction<Vulnerable>(unit, default(int?), num, default(int?), default(int?), 0f, true);
				}
				List<EnemyUnit>.Enumerator enumerator2 = default(List<EnemyUnit>.Enumerator);
			}
			base.Blackout = true;
			yield break;
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
