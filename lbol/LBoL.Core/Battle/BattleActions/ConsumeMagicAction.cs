using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class ConsumeMagicAction : EventBattleAction<DollMagicArgs>
	{
		public ConsumeMagicAction(Doll doll)
		{
			base.Args = new DollMagicArgs
			{
				Doll = doll,
				Magic = doll.MagicCost,
				CanCancel = false
			};
		}
		public ConsumeMagicAction(Doll doll, int magic)
		{
			base.Args = new DollMagicArgs
			{
				Doll = doll,
				Magic = magic,
				CanCancel = false
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Main", delegate
			{
				base.Args.Doll.ConsumeMagic(base.Args.Magic);
			}, true);
			yield break;
		}
	}
}
