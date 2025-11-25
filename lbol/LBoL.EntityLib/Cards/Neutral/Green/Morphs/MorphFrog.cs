using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
namespace LBoL.EntityLib.Cards.Neutral.Green.Morphs
{
	[UsedImplicitly]
	public sealed class MorphFrog : OptionCard
	{
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
			BattleController battle = base.Battle;
			int num = battle.LimaoSchoolFrogTimes + 1;
			battle.LimaoSchoolFrogTimes = num;
			yield break;
		}
	}
}
