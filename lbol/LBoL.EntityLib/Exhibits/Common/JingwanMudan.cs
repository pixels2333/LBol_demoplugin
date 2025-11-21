using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000178 RID: 376
	[UsedImplicitly]
	public sealed class JingwanMudan : Exhibit
	{
		// Token: 0x0600053D RID: 1341 RVA: 0x0000CF5A File Offset: 0x0000B15A
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}

		// Token: 0x0600053E RID: 1342 RVA: 0x0000CF79 File Offset: 0x0000B179
		private IEnumerable<BattleAction> OnManaConsumed(ManaEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.Counter += args.Value.Red;
			base.Counter += args.Value.Philosophy;
			ValueTuple<int, int> valueTuple = base.Counter.DivRem(base.Value1);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			base.Counter = item2;
			if (item != 0)
			{
				base.NotifyActivating();
				yield return new DamageAction(base.Owner, base.Battle.EnemyGroup.Alives, DamageInfo.Reaction((float)(base.Value2 * item), false), "ExhMudan", GunType.Single);
			}
			yield break;
		}

		// Token: 0x0600053F RID: 1343 RVA: 0x0000CF90 File Offset: 0x0000B190
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
