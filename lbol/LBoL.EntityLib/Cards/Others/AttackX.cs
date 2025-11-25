using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.EntityLib.Cards.Others
{
	[UsedImplicitly]
	public sealed class AttackX : Card
	{
		public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
		{
			return pooledMana;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int num = base.SynergyAmount(consumingMana, ManaColor.Any, 1);
			EnemyUnit target = selector.SelectedEnemy;
			if (num > 0)
			{
				bool flag = Random.Range(0f, 1f) > 0.5f;
				Guns guns = new Guns(flag ? "推进之银up" : "推进之银down");
				for (int i = 1; i < num; i++)
				{
					guns.Add(flag ? ((i % 2 == 0) ? "推进之银up" : "推进之银down") : ((i % 2 == 1) ? "推进之银up" : "推进之银down"));
				}
				foreach (GunPair gunPair in guns.GunPairs)
				{
					if (!target.IsAlive)
					{
						if (base.Battle.BattleShouldEnd)
						{
							yield return new EndShootAction(base.Battle.Player);
							yield break;
						}
						target = base.Battle.AllAliveEnemies.Sample(base.GameRun.BattleRng);
					}
					yield return new DamageAction(base.Battle.Player, target, this.Damage, gunPair.GunName, gunPair.GunType);
				}
				List<GunPair>.Enumerator enumerator = default(List<GunPair>.Enumerator);
			}
			yield break;
			yield break;
		}
		private const string UpGun = "推进之银up";
		private const string DownGun = "推进之银down";
	}
}
