using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Marisa
{
	public sealed class ScryBlockSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<ScryEventArgs>(base.Battle.Scrying, delegate(ScryEventArgs args)
			{
				if (args.Cause != ActionCause.OnlyCalculate)
				{
					base.React(new LazySequencedReactor(this.OnScrying));
				}
			});
		}
		private IEnumerable<BattleAction> OnScrying()
		{
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Battle.Player, base.Battle.Player, base.Level, 0, BlockShieldType.Direct, true);
			yield break;
		}
	}
}
