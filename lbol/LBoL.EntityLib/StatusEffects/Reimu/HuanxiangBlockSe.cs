using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x0200002C RID: 44
	[UsedImplicitly]
	public sealed class HuanxiangBlockSe : StatusEffect
	{
		// Token: 0x06000078 RID: 120 RVA: 0x00002CA4 File Offset: 0x00000EA4
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardExiled, new EventSequencedReactor<CardEventArgs>(this.OnCardExiled));
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00002CC3 File Offset: 0x00000EC3
		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Battle.Player, base.Level, 0, BlockShieldType.Direct, false);
			yield break;
		}
	}
}
