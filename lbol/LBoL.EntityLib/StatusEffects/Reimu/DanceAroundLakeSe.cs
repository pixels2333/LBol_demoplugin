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
	// Token: 0x02000028 RID: 40
	[UsedImplicitly]
	public sealed class DanceAroundLakeSe : StatusEffect
	{
		// Token: 0x0600006A RID: 106 RVA: 0x00002B74 File Offset: 0x00000D74
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardExiled, new EventSequencedReactor<CardEventArgs>(this.OnCardExiled));
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00002B93 File Offset: 0x00000D93
		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			if (args.Cause != ActionCause.AutoExile)
			{
				base.NotifyActivating();
				yield return new DrawManyCardAction(base.Level);
			}
			yield break;
		}
	}
}
