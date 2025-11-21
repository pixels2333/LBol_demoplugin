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
	// Token: 0x02000025 RID: 37
	[UsedImplicitly]
	public sealed class AutoQumobangSe : StatusEffect
	{
		// Token: 0x0600005E RID: 94 RVA: 0x000029CC File Offset: 0x00000BCC
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

		// Token: 0x0600005F RID: 95 RVA: 0x00002A65 File Offset: 0x00000C65
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

		// Token: 0x06000060 RID: 96 RVA: 0x00002A75 File Offset: 0x00000C75
		protected override void OnRemoved(Unit unit)
		{
			base.Battle.DrawCardCount -= base.Level;
		}

		// Token: 0x04000003 RID: 3
		private bool _addedInStartTurnDrawing;
	}
}
