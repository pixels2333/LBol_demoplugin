using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000143 RID: 323
	[UsedImplicitly]
	public sealed class YueGujiu : ShiningExhibit
	{
		// Token: 0x06000470 RID: 1136 RVA: 0x0000BC55 File Offset: 0x00009E55
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x0000BC74 File Offset: 0x00009E74
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn && args.Card.IsExile && args.ConsumingMana != ManaGroup.Empty)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
