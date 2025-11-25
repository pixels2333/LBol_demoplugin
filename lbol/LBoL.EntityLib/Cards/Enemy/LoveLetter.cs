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
using LBoL.EntityLib.StatusEffects.Enemy;
using UnityEngine;
namespace LBoL.EntityLib.Cards.Enemy
{
	[UsedImplicitly]
	public sealed class LoveLetter : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<EnemyUnit> list = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.Battle.EnemyGroup, (EnemyUnit u) => u is LoveGirl && u.IsAlive));
			if (list.Count > 1)
			{
				Debug.LogWarning("Multiple LoveGirl exists");
			}
			else if (list.Count == 0)
			{
				Debug.LogWarning("Payment is used while no LoveGirl");
			}
			else
			{
				EnemyUnit loveGirl = Enumerable.First<EnemyUnit>(list);
				if (!Enumerable.Any<Card>(base.Battle.BattleCardUsageHistory, (Card card) => card is LoveLetter))
				{
					yield return PerformAction.Chat(base.Battle.Player, "Chat.LoveGirlPlayer".Localize(true), 2f, 0f, 0f, true);
					yield return PerformAction.Chat(loveGirl, "Chat.LoveGirlRespond".Localize(true), 2f, 1.5f, 0f, true);
				}
				yield return base.DebuffAction<LoveGirlDamageIncrease>(loveGirl, 1, 0, 0, 0, true, 0.2f);
				loveGirl = null;
			}
			yield break;
		}
	}
}
