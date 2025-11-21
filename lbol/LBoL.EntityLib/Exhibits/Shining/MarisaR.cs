using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000133 RID: 307
	[UsedImplicitly]
	public sealed class MarisaR : ShiningExhibit
	{
		// Token: 0x06000435 RID: 1077 RVA: 0x0000B566 File Offset: 0x00009766
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x0000B585 File Offset: 0x00009785
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Attack && (args.ConsumingMana.Red > 0 || args.ConsumingMana.Philosophy > 0))
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Charging>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
