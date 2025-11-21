using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Marisa
{
	// Token: 0x0200006E RID: 110
	public sealed class ScryBlockSe : StatusEffect
	{
		// Token: 0x0600017E RID: 382 RVA: 0x00004F9D File Offset: 0x0000319D
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

		// Token: 0x0600017F RID: 383 RVA: 0x00004FBC File Offset: 0x000031BC
		private IEnumerable<BattleAction> OnScrying()
		{
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Battle.Player, base.Battle.Player, base.Level, 0, BlockShieldType.Direct, true);
			yield break;
		}
	}
}
