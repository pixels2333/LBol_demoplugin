using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.Green
{
	// Token: 0x0200005C RID: 92
	public sealed class RangziFanshuSe : StatusEffect
	{
		// Token: 0x06000140 RID: 320 RVA: 0x0000474C File Offset: 0x0000294C
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.BattleEnding, new EventSequencedReactor<GameEventArgs>(this.OnBattleEnding));
		}

		// Token: 0x06000141 RID: 321 RVA: 0x0000476B File Offset: 0x0000296B
		private IEnumerable<BattleAction> OnBattleEnding(GameEventArgs args)
		{
			if (base.Battle.Player.IsAlive)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Battle.Player, base.Battle.Player, base.Level, HealType.Normal, 0.2f);
			}
			yield break;
		}
	}
}
