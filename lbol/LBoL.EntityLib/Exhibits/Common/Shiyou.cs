using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000190 RID: 400
	[UsedImplicitly]
	public sealed class Shiyou : Exhibit
	{
		// Token: 0x060005A4 RID: 1444 RVA: 0x0000D95D File Offset: 0x0000BB5D
		protected override void OnEnterBattle()
		{
			this._remainTimes = base.Value3;
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}

		// Token: 0x060005A5 RID: 1445 RVA: 0x0000D988 File Offset: 0x0000BB88
		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			if (this._remainTimes > 0)
			{
				base.Counter += args.Value.Black;
				base.Counter += args.Value.Philosophy;
				ValueTuple<int, int> valueTuple = base.Counter.DivRem(base.Value1);
				int div = valueTuple.Item1;
				int item = valueTuple.Item2;
				base.Counter = item;
				if (div != 0)
				{
					div = Math.Min(div, this._remainTimes);
					base.NotifyActivating();
					yield return new GainMoneyAction(base.Value2 * div, SpecialSourceType.None);
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

		// Token: 0x060005A6 RID: 1446 RVA: 0x0000D99F File Offset: 0x0000BB9F
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
			base.Blackout = false;
		}

		// Token: 0x04000040 RID: 64
		private int _remainTimes;
	}
}
