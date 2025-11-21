using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000165 RID: 357
	public sealed class ConsumeMagicAction : EventBattleAction<DollMagicArgs>
	{
		// Token: 0x06000DE3 RID: 3555 RVA: 0x00026402 File Offset: 0x00024602
		public ConsumeMagicAction(Doll doll)
		{
			base.Args = new DollMagicArgs
			{
				Doll = doll,
				Magic = doll.MagicCost,
				CanCancel = false
			};
		}

		// Token: 0x06000DE4 RID: 3556 RVA: 0x0002642F File Offset: 0x0002462F
		public ConsumeMagicAction(Doll doll, int magic)
		{
			base.Args = new DollMagicArgs
			{
				Doll = doll,
				Magic = magic,
				CanCancel = false
			};
		}

		// Token: 0x06000DE5 RID: 3557 RVA: 0x00026457 File Offset: 0x00024657
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
