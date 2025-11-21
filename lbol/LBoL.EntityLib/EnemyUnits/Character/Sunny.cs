using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x0200024A RID: 586
	[UsedImplicitly]
	public sealed class Sunny : LightFairy
	{
		// Token: 0x06000955 RID: 2389 RVA: 0x0001420C File Offset: 0x0001240C
		protected override void OnEnterBattle(BattleController battle)
		{
			base.OnEnterBattle(battle);
			LightFairy lightFairy = null;
			LightFairy lightFairy2 = null;
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				LightFairy lightFairy3 = (LightFairy)enemyUnit;
				if (!(lightFairy3 is Luna))
				{
					if (lightFairy3 is Star)
					{
						lightFairy2 = lightFairy3;
					}
				}
				else
				{
					lightFairy = lightFairy3;
				}
			}
			List<LightFairy> list = new List<LightFairy>();
			list.Add(this);
			list.Add(lightFairy);
			list.Add(lightFairy2);
			List<LightFairy> list2 = list;
			list2.Shuffle(base.EnemyMoveRng);
			list2[0].Spell();
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				list2[1].Spell();
			}
			else
			{
				list2[1].Light();
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x000142FC File Offset: 0x000124FC
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			EnemyUnit luna = null;
			EnemyUnit star = null;
			foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
			{
				if (!(enemyUnit is Luna))
				{
					if (enemyUnit is Star)
					{
						star = enemyUnit;
					}
				}
				else
				{
					luna = enemyUnit;
				}
			}
			string text;
			if (Random.Range(0f, 1f) > 0.5f)
			{
				text = "Chat.Sunny1".LocalizeFormat(new object[] { base.Battle.Player.GetName() });
			}
			else
			{
				text = "Chat.Sunny2".Localize(true);
			}
			yield return PerformAction.Chat(this, text, 3.2f, 0.5f, 0f, true);
			yield return PerformAction.Chat(luna, "Chat.Luna".Localize(true), 2f, 4f, 0f, true);
			yield return PerformAction.Chat(star, "Chat.Star".Localize(true), 2f, 4f, 0f, true);
			yield break;
		}
	}
}
