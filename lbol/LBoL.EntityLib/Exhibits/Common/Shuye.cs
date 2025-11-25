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
	[UsedImplicitly]
	public sealed class Shuye : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}
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
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
