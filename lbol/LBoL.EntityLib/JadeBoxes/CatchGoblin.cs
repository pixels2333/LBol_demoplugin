using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class CatchGoblin : JadeBox
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			Unit randomAliveEnemy = base.Battle.RandomAliveEnemy;
			int? num = new int?(base.Value1);
			yield return new ApplyStatusEffectAction<CatchGoblinSe>(randomAliveEnemy, default(int?), num, default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
