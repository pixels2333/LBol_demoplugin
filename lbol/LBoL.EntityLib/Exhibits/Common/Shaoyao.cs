using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200018D RID: 397
	[UsedImplicitly]
	public sealed class Shaoyao : Exhibit
	{
		// Token: 0x0600059A RID: 1434 RVA: 0x0000D890 File Offset: 0x0000BA90
		protected override void OnEnterBattle()
		{
			this._remainTimes = base.Value3;
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}

		// Token: 0x0600059B RID: 1435 RVA: 0x0000D8BB File Offset: 0x0000BABB
		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			if (this._remainTimes > 0)
			{
				base.Counter += args.Value.White;
				base.Counter += args.Value.Philosophy;
				ValueTuple<int, int> valueTuple = base.Counter.DivRem(base.Value1);
				int div = valueTuple.Item1;
				int item = valueTuple.Item2;
				base.Counter = item;
				if (div != 0)
				{
					div = Math.Min(div, this._remainTimes);
					base.NotifyActivating();
					yield return new HealAction(base.Owner, base.Owner, base.Value2 * div, HealType.Normal, 0.2f);
					this._remainTimes -= div;
					if (this._remainTimes <= 0)
					{
						base.Counter = 0;
						base.Blackout = true;
					}
				}
			}
			yield break;
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x0000D8D2 File Offset: 0x0000BAD2
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
			base.Blackout = false;
		}

		// Token: 0x0400003F RID: 63
		private int _remainTimes;
	}
}
