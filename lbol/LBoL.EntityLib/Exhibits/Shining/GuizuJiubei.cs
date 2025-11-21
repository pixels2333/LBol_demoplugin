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
	// Token: 0x0200012A RID: 298
	[UsedImplicitly]
	public sealed class GuizuJiubei : ShiningExhibit
	{
		// Token: 0x06000416 RID: 1046 RVA: 0x0000B28A File Offset: 0x0000948A
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
			base.ReactBattleEvent<UnitEventArgs>(base.Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarting));
		}

		// Token: 0x06000417 RID: 1047 RVA: 0x0000B2C8 File Offset: 0x000094C8
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Attack)
			{
				int num = base.Counter + 1;
				base.Counter = num;
			}
		}

		// Token: 0x06000418 RID: 1048 RVA: 0x0000B2F3 File Offset: 0x000094F3
		private IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
		{
			if (base.Counter > 0)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(base.Counter), default(int?), default(int?), default(int?), 0f, true);
				base.Counter = 0;
			}
			yield break;
		}

		// Token: 0x06000419 RID: 1049 RVA: 0x0000B303 File Offset: 0x00009503
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
		}
	}
}
