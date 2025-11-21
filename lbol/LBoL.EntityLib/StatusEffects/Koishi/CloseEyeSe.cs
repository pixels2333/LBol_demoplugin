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
	// Token: 0x02000073 RID: 115
	[UsedImplicitly]
	public sealed class CloseEyeSe : StatusEffect
	{
		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600018C RID: 396 RVA: 0x00005134 File Offset: 0x00003334
		// (set) Token: 0x0600018D RID: 397 RVA: 0x0000513C File Offset: 0x0000333C
		private bool Hidden { get; set; }

		// Token: 0x0600018E RID: 398 RVA: 0x00005145 File Offset: 0x00003345
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<GameEventArgs>(base.Battle.RoundStarting, new GameEventHandler<GameEventArgs>(this.OnRoundStarting));
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnStarting, new EventSequencedReactor<GameEventArgs>(this.OnEnemyTurnStarting));
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00005184 File Offset: 0x00003384
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

		// Token: 0x06000190 RID: 400 RVA: 0x000051C5 File Offset: 0x000033C5
		private IEnumerable<BattleAction> OnEnemyTurnStarting(GameEventArgs args)
		{
			if (base.Level <= 0)
			{
				yield return new RemoveStatusEffectAction(this, true, 0.1f);
			}
			yield break;
		}

		// Token: 0x06000191 RID: 401 RVA: 0x000051D8 File Offset: 0x000033D8
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
