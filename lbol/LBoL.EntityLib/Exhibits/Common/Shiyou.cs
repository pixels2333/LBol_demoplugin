using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Shiyou : Exhibit
	{
		protected override void OnEnterBattle()
		{
			this._remainTimes = base.Value3;
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}
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
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
			base.Blackout = false;
		}
		private int _remainTimes;
	}
}
