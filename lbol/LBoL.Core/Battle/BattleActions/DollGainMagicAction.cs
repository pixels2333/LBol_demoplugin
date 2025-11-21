using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200016E RID: 366
	public sealed class DollGainMagicAction : EventBattleAction<DollMagicEventArgs>
	{
		// Token: 0x06000E23 RID: 3619 RVA: 0x00026F9F File Offset: 0x0002519F
		public DollGainMagicAction(Doll doll, int magic)
		{
			base.Args = new DollMagicEventArgs
			{
				Doll = doll,
				Magic = magic
			};
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x00026FC0 File Offset: 0x000251C0
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
