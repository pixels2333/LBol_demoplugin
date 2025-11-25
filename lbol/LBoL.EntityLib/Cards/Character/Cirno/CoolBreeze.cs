using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class CoolBreeze : Card
	{
		public override bool Triggered
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Any<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.HasStatusEffect<Cold>());
				}
				return false;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.TriggeredAnyhow)
			{
				yield return base.UpgradeRandomHandAction(base.Value1, CardType.Unknown);
			}
			yield break;
		}
	}
}
