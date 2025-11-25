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
	public sealed class CloseEyeSe : StatusEffect
	{
		private bool Hidden { get; set; }
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<GameEventArgs>(base.Battle.RoundStarting, new GameEventHandler<GameEventArgs>(this.OnRoundStarting));
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(this.OnEnemyTurnStarting));
		}
		private void OnRoundStarting(GameEventArgs args)
		{
			int num = base.Level - 1;
			base.Level = num;
			if (!this.Hidden)
			{
				this.Hidden = true;
				BattleController battle = base.Battle;
				num = battle.HideEnemyIntentionLevel + 1;
				battle.HideEnemyIntentionLevel = num;
			}
		}
		private IEnumerable<BattleAction> OnEnemyTurnStarting(GameEventArgs args)
		{
			if (base.Level <= 0)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}
		protected override void OnRemoving(Unit unit)
		{
			if (this.Hidden)
			{
				this.Hidden = false;
				BattleController battle = base.Battle;
				int num = battle.HideEnemyIntentionLevel - 1;
				battle.HideEnemyIntentionLevel = num;
				if (!base.Battle.HideEnemyIntention)
				{
					base.Battle.RevealHiddenIntentions();
				}
			}
		}
	}
}
