using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000049 RID: 73
	public sealed class SeeFengshuiSe : StatusEffect
	{
		// Token: 0x1700000F RID: 15
		// (get) Token: 0x060000E3 RID: 227 RVA: 0x0000397B File Offset: 0x00001B7B
		[UsedImplicitly]
		public ScryInfo Scry
		{
			get
			{
				return new ScryInfo(base.Level);
			}
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00003988 File Offset: 0x00001B88
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting));
			base.ReactOwnerEvent<CardMovingEventArgs>(base.Battle.CardMoved, new EventSequencedReactor<CardMovingEventArgs>(this.OnCardMoved));
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x000039D4 File Offset: 0x00001BD4
		private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ScryAction(this.Scry);
			yield break;
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x000039E4 File Offset: 0x00001BE4
		private IEnumerable<BattleAction> OnCardMoved(CardMovingEventArgs args)
		{
			if (args.ActionSource == this && args.Card.CardType == CardType.Defense)
			{
				yield return new CastBlockShieldAction(base.Battle.Player, new BlockInfo(this.Block, BlockShieldType.Direct), false);
			}
			yield break;
		}

		// Token: 0x04000005 RID: 5
		[UsedImplicitly]
		public int Block = 5;
	}
}
