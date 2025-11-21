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
	// Token: 0x02000035 RID: 53
	[UsedImplicitly]
	public sealed class ReverseJiejieSe : StatusEffect
	{
		// Token: 0x0600009C RID: 156 RVA: 0x000031D8 File Offset: 0x000013D8
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatusEffectEventArgs>(base.Battle.Player.StatusEffectRemoved, new EventSequencedReactor<StatusEffectEventArgs>(this.StatusEffectRemoved));
		}

		// Token: 0x0600009D RID: 157 RVA: 0x000031FC File Offset: 0x000013FC
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
