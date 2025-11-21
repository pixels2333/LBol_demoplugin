using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x02000349 RID: 841
	[UsedImplicitly]
	public sealed class FanzhuanChongdong : Card
	{
		// Token: 0x17000160 RID: 352
		// (get) Token: 0x06000C3C RID: 3132 RVA: 0x00017FC7 File Offset: 0x000161C7
		// (set) Token: 0x06000C3D RID: 3133 RVA: 0x00017FCF File Offset: 0x000161CF
		private bool LifeGained { get; set; }

		// Token: 0x06000C3E RID: 3134 RVA: 0x00017FD8 File Offset: 0x000161D8
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnTurnStarting), (GameEventPriority)0);
			base.HandleBattleEvent<HealEventArgs>(base.Battle.Player.HealingReceived, new GameEventHandler<HealEventArgs>(this.OnHealingReceived), (GameEventPriority)0);
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C3F RID: 3135 RVA: 0x0001804D File Offset: 0x0001624D
		private void OnTurnStarting(UnitEventArgs args)
		{
			this.LifeGained = false;
		}

		// Token: 0x06000C40 RID: 3136 RVA: 0x00018056 File Offset: 0x00016256
		private void OnHealingReceived(HealEventArgs args)
		{
			this.LifeGained = true;
		}

		// Token: 0x06000C41 RID: 3137 RVA: 0x0001805F File Offset: 0x0001625F
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Zone == CardZone.Hand && !this.LifeGained)
			{
				base.NotifyActivating();
				yield return base.LoseLifeAction(base.Value1);
			}
			yield break;
		}
	}
}
