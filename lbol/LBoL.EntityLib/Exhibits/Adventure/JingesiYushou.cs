using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class JingesiYushou : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.Active = true;
			base.ReactBattleEvent<StatusEffectApplyEventArgs>(base.GameRun.Player.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}
		private IEnumerable<BattleAction> OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			if (base.Active && args.Effect.Type == StatusEffectType.Negative)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Battle.Player, 0, base.Value1, BlockShieldType.Normal, false);
				base.Active = false;
				base.Blackout = true;
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
	}
}
