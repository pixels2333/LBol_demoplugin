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
	public sealed class SelfControlSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Battle.Player.HasStatusEffect<MoodPeace>())
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Battle.Player, base.Count, 0, BlockShieldType.Direct, false);
			}
			else if (base.Battle.Player.HasStatusEffect<MoodPassion>())
			{
				base.NotifyActivating();
				yield return base.BuffAction<Firepower>(base.Level, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
