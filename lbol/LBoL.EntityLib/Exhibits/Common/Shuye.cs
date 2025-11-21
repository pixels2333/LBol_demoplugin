using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000195 RID: 405
	[UsedImplicitly]
	public sealed class Shuye : Exhibit
	{
		// Token: 0x060005BB RID: 1467 RVA: 0x0000DB13 File Offset: 0x0000BD13
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}

		// Token: 0x060005BC RID: 1468 RVA: 0x0000DB32 File Offset: 0x0000BD32
		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			base.Counter += args.Value.Green;
			base.Counter += args.Value.Philosophy;
			ValueTuple<int, int> valueTuple = base.Counter.DivRem(base.Value1);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			base.Counter = item2;
			if (item != 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Graze>(base.Battle.Player, new int?(item), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x060005BD RID: 1469 RVA: 0x0000DB49 File Offset: 0x0000BD49
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
