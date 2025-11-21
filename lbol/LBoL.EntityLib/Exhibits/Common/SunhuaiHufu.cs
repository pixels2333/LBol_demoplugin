using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000197 RID: 407
	[UsedImplicitly]
	public sealed class SunhuaiHufu : Exhibit
	{
		// Token: 0x060005C3 RID: 1475 RVA: 0x0000DB9A File Offset: 0x0000BD9A
		protected override string GetBaseDescription()
		{
			if (base.GameRun == null || !Enumerable.Contains<Exhibit>(base.GameRun.Player.Exhibits, this))
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x060005C4 RID: 1476 RVA: 0x0000DBCC File Offset: 0x0000BDCC
		protected override void OnAdded(PlayerUnit player)
		{
			this.RefreshCounter();
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsAdded, delegate(CardsEventArgs _)
			{
				this.RefreshCounter();
			});
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsRemoved, delegate(CardsEventArgs _)
			{
				this.RefreshCounter();
			});
		}

		// Token: 0x060005C5 RID: 1477 RVA: 0x0000DC19 File Offset: 0x0000BE19
		private void RefreshCounter()
		{
			if (base.GameRun != null)
			{
				base.Counter = Enumerable.Count<Card>(base.GameRun.BaseDeck, (Card card) => card.CardType == CardType.Misfortune);
			}
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x0000DC58 File Offset: 0x0000BE58
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				base.Blackout = true;
			});
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x0000DCA4 File Offset: 0x0000BEA4
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Counter > 0)
			{
				int level = base.Counter * base.Value1;
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(level), default(int?), default(int?), default(int?), 0f, true);
				yield return new ApplyStatusEffectAction<Spirit>(base.Owner, new int?(level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x0000DCB4 File Offset: 0x0000BEB4
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
