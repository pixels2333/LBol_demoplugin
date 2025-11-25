using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Koishi
{
	[UsedImplicitly]
	public sealed class MoodChangeBlockSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<MoodChangeEventArgs>(base.Battle.Player.MoodChanged, new EventSequencedReactor<MoodChangeEventArgs>(this.OnPlayerMoodChanged));
		}
		private IEnumerable<BattleAction> OnPlayerMoodChanged(MoodChangeEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Battle.Player, base.Level, 0, BlockShieldType.Direct, false);
			yield break;
		}
	}
}
