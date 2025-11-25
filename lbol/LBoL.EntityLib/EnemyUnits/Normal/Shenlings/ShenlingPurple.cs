using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Shenlings
{
	[UsedImplicitly]
	public sealed class ShenlingPurple : Shenling
	{
		protected override IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			foreach (BattleAction battleAction in base.OnBattleStarted(arg))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new ApplyStatusEffectAction<ShenlingHp>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
			yield break;
		}
	}
}
