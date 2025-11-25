using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class SakuyaDetective : Card
	{
		[UsedImplicitly]
		public int BossDamage
		{
			get
			{
				GameRunController gameRun = base.GameRun;
				if (gameRun == null)
				{
					return 0;
				}
				return gameRun.FinalBossInitialDamage;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.GameRun.FinalBossInitialDamage += base.Value1;
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
