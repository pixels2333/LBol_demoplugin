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
	[UsedImplicitly]
	public sealed class JingwanMudan : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<ManaEventArgs>(base.Battle.ManaConsumed, new EventSequencedReactor<ManaEventArgs>(this.OnManaConsumed));
		}
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
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
