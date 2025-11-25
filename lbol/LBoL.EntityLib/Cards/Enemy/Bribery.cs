using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.StatusEffects.Enemy;
using UnityEngine;
namespace LBoL.EntityLib.Cards.Enemy
{
	[UsedImplicitly]
	public sealed class Bribery : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.Battle.EnemyGroup, (EnemyUnit u) => u is Long && u.IsAlive));
			if (list.Count > 1)
			{
				Debug.LogWarning("Multiple Long exists");
			}
			else if (list.Count == 0)
			{
				Debug.LogWarning("Bribery is used while no Long");
			}
			else
			{
				EnemyUnit longUnit = Enumerable.First<EnemyUnit>(list);
				yield return PerformAction.Chat(longUnit, ("Chat.LongBribery" + (Enumerable.Count<Card>(base.Battle.BattleCardUsageHistory, (Card card) => card is Bribery) % 2 + 1).ToString()).Localize(true), 3f, 0f, 0f, true);
				yield return base.DebuffAction<LongEscape>(longUnit, 80, 0, 0, 0, true, 0.2f);
				yield return base.DebuffAction<FirepowerNegative>(longUnit, 1, 0, 0, 0, true, 0.2f);
				longUnit = null;
			}
			yield break;
		}
	}
}
