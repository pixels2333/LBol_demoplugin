using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x02000348 RID: 840
	[UsedImplicitly]
	public sealed class Drunk : Card
	{
		// Token: 0x06000C36 RID: 3126 RVA: 0x00017F08 File Offset: 0x00016108
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Count = base.Battle.TurnCardUsageHistory.Count;
			this.NotifyChanged();
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed), (GameEventPriority)0);
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new GameEventHandler<UnitEventArgs>(this.OnTurnEnded), (GameEventPriority)0);
		}

		// Token: 0x06000C37 RID: 3127 RVA: 0x00017F72 File Offset: 0x00016172
		private void OnCardUsed(CardUsingEventArgs args)
		{
			this.Count = base.Battle.TurnCardUsageHistory.Count;
			this.NotifyChanged();
		}

		// Token: 0x06000C38 RID: 3128 RVA: 0x00017F90 File Offset: 0x00016190
		private void OnTurnEnded(UnitEventArgs args)
		{
			this.Count = 0;
			this.NotifyChanged();
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x00017F9F File Offset: 0x0001619F
		public override bool ShouldPreventOtherCardUsage(Card card)
		{
			return this.Count >= base.Value1;
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000C3A RID: 3130 RVA: 0x00017FB2 File Offset: 0x000161B2
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.CardDrunk".Localize(true);
			}
		}

		// Token: 0x040000FE RID: 254
		[UsedImplicitly]
		public int Count;
	}
}
