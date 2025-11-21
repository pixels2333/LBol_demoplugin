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
	// Token: 0x02000172 RID: 370
	[UsedImplicitly]
	public sealed class HuliMianju : Exhibit
	{
		// Token: 0x06000523 RID: 1315 RVA: 0x0000CCD8 File Offset: 0x0000AED8
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, delegate(UnitEventArgs _)
			{
				base.Counter = 0;
				base.Active = false;
			});
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x0000CD24 File Offset: 0x0000AF24
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
			base.Active = false;
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x0000CD34 File Offset: 0x0000AF34
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn && args.Card.CardType == CardType.Attack)
			{
				base.Counter = (base.Counter + 1) % base.Value1;
				if (base.Counter == 1)
				{
					base.Active = true;
				}
				if (base.Counter == 0)
				{
					base.NotifyActivating();
					yield return new ApplyStatusEffectAction<Spirit>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			yield break;
		}
	}
}
