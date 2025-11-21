using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200019A RID: 410
	[UsedImplicitly]
	public sealed class TiangouDunpai : Exhibit
	{
		// Token: 0x060005D4 RID: 1492 RVA: 0x0000DD4C File Offset: 0x0000BF4C
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, delegate(UnitEventArgs _)
			{
				base.Active = true;
			});
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x060005D5 RID: 1493 RVA: 0x0000DD98 File Offset: 0x0000BF98
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Active && args.Card.CardType == CardType.Defense)
			{
				base.NotifyActivating();
				base.Active = false;
				yield return new ApplyStatusEffectAction<TempFirepower>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x0000DDAF File Offset: 0x0000BFAF
		protected override void OnLeaveBattle()
		{
			base.Active = false;
		}
	}
}
