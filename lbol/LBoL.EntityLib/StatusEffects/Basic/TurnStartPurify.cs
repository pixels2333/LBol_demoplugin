using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Basic
{
	[UsedImplicitly]
	public sealed class TurnStartPurify : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted), (GameEventPriority)99999);
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnding));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			if (base.Battle.BattleMana.HasTrivial)
			{
				yield return ConvertManaAction.Purify(base.Battle.BattleMana, base.Level);
			}
			yield break;
		}
		private void OnPlayerTurnEnding(UnitEventArgs args)
		{
			this.React(new RemoveStatusEffectAction(this, true, 0.1f));
		}
	}
}
