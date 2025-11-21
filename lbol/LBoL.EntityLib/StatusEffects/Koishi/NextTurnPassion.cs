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
	// Token: 0x0200007F RID: 127
	[UsedImplicitly]
	public sealed class NextTurnPassion : StatusEffect
	{
		// Token: 0x060001C4 RID: 452 RVA: 0x00005828 File Offset: 0x00003A28
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000584C File Offset: 0x00003A4C
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<MoodPassion>(base.Battle.Player, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield return new DrawManyCardAction(base.Level);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
