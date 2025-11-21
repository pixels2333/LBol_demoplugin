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
	// Token: 0x0200007B RID: 123
	[UsedImplicitly]
	public sealed class MoodChangeBlockSe : StatusEffect
	{
		// Token: 0x060001AA RID: 426 RVA: 0x00005491 File Offset: 0x00003691
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<MoodChangeEventArgs>(base.Battle.Player.MoodChanged, new EventSequencedReactor<MoodChangeEventArgs>(this.OnPlayerMoodChanged));
		}

		// Token: 0x060001AB RID: 427 RVA: 0x000054B5 File Offset: 0x000036B5
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
