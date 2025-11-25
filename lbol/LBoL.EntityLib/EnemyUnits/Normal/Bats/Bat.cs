using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
namespace LBoL.EntityLib.EnemyUnits.Normal.Bats
{
	[UsedImplicitly]
	public sealed class Bat : BatOrigin
	{
		protected override IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			foreach (BattleAction battleAction in base.OnBattleStarted(arg))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
