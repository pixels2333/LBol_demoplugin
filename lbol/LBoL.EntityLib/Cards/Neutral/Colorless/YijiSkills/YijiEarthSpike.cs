using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Neutral.Colorless.YijiSkills
{
	public sealed class YijiEarthSpike : OptionCard
	{
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
