using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal;
using UnityEngine;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x0200036A RID: 874
	[UsedImplicitly]
	public sealed class Payment : Card
	{
		// Token: 0x06000C92 RID: 3218 RVA: 0x000185C8 File Offset: 0x000167C8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.Battle.EnemyGroup, (EnemyUnit u) => u is FraudRabbit && u.IsAlive));
			if (list.Count > 1)
			{
				Debug.LogWarning("Multiple FraudRabbit exists");
			}
			else if (list.Count == 0)
			{
				Debug.LogWarning("Payment is used while no FraudRabbit");
			}
			else
			{
				EnemyUnit rabbit = Enumerable.First<EnemyUnit>(list);
				yield return PerformAction.Chat(base.Battle.Player, "Chat.FraudRabbitPlayer".Localize(true), 3f, 0f, 0f, true);
				yield return new EscapeAction(rabbit);
				rabbit = null;
			}
			yield break;
		}
	}
}
