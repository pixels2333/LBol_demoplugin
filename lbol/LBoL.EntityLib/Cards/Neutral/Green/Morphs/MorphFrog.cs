using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;

namespace LBoL.EntityLib.Cards.Neutral.Green.Morphs
{
	// Token: 0x02000307 RID: 775
	[UsedImplicitly]
	public sealed class MorphFrog : OptionCard
	{
		// Token: 0x06000B86 RID: 2950 RVA: 0x000171DB File Offset: 0x000153DB
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
