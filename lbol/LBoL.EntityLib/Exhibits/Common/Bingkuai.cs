using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000158 RID: 344
	[UsedImplicitly]
	public sealed class Bingkuai : Exhibit
	{
		// Token: 0x060004B7 RID: 1207 RVA: 0x0000C321 File Offset: 0x0000A521
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x0000C340 File Offset: 0x0000A540
		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			base.Counter += args.Value.Blue;
			base.Counter += args.Value.Philosophy;
			ValueTuple<int, int> valueTuple = base.Counter.DivRem(base.Value1);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			base.Counter = item2;
			if (item != 0)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Owner, base.Owner, 0, base.Value2 * item, BlockShieldType.Normal, false);
			}
			yield break;
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x0000C357 File Offset: 0x0000A557
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
