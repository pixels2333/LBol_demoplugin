using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Colorless.YijiSkills
{
	// Token: 0x0200030D RID: 781
	public sealed class YijiEarthSpike : OptionCard
	{
		// Token: 0x06000B96 RID: 2966 RVA: 0x00017385 File Offset: 0x00015585
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return base.AttackAction(base.Battle.AllAliveEnemies);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (BattleAction battleAction in base.DebuffAction<Weak>(base.Battle.AllAliveEnemies, 0, base.Value1, 0, 0, true, 0.2f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
