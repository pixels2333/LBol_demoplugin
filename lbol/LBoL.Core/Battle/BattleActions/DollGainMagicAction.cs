using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class DollGainMagicAction : EventBattleAction<DollMagicEventArgs>
	{
		public DollGainMagicAction(Doll doll, int magic)
		{
			base.Args = new DollMagicEventArgs
			{
				Doll = doll,
				Magic = magic
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("DollAction", delegate
			{
				Doll doll = base.Args.Doll;
				if (doll.HasMagic)
				{
					doll.Magic = Math.Clamp(doll.Magic + base.Args.Magic, 0, doll.MaxMagic);
				}
			}, false);
			yield break;
		}
	}
}
