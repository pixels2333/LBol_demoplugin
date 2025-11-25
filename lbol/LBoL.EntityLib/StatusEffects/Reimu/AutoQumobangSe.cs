using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.EntityLib.StatusEffects.Reimu
{
	[UsedImplicitly]
	public sealed class AutoQumobangSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			if (base.Level <= 0 || base.Count <= 0)
			{
				Debug.LogWarning(this.DebugName + " added incorrectly.");
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			}
			if (base.Battle.StartTurnDrawing)
			{
				this._addedInStartTurnDrawing = true;
			}
			base.Battle.DrawCardCount += base.Level;
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (this._addedInStartTurnDrawing)
			{
				this._addedInStartTurnDrawing = false;
			}
			else
			{
				int num = base.Count - 1;
				base.Count = num;
				base.NotifyActivating();
				if (base.Count <= 0)
				{
					yield return new RemoveStatusEffectAction(this, true, 0.1f);
				}
			}
			yield break;
		}
		protected override void OnRemoved(Unit unit)
		{
			base.Battle.DrawCardCount -= base.Level;
		}
		private bool _addedInStartTurnDrawing;
	}
}
