using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Reimu
{
	[UsedImplicitly]
	public sealed class ReverseJiejieSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatusEffectEventArgs>(base.Battle.Player.StatusEffectRemoved, new EventSequencedReactor<StatusEffectEventArgs>(this.StatusEffectRemoved));
		}
		private IEnumerable<BattleAction> StatusEffectRemoved(StatusEffectEventArgs args)
		{
			if (args.Effect.Type == StatusEffectType.Positive)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Battle.Player, 0, base.Level, BlockShieldType.Direct, false);
			}
			yield break;
		}
	}
}
