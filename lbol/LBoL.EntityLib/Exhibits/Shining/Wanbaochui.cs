using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000140 RID: 320
	[UsedImplicitly]
	public sealed class Wanbaochui : ShiningExhibit
	{
		// Token: 0x06000463 RID: 1123 RVA: 0x0000BAA0 File Offset: 0x00009CA0
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.WanbaochuiFlag + 1;
			gameRun.WanbaochuiFlag = num;
		}

		// Token: 0x06000464 RID: 1124 RVA: 0x0000BAC4 File Offset: 0x00009CC4
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.WanbaochuiFlag - 1;
			gameRun.WanbaochuiFlag = num;
		}

		// Token: 0x06000465 RID: 1125 RVA: 0x0000BAE6 File Offset: 0x00009CE6
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsing, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x0000BB08 File Offset: 0x00009D08
		private void OnCardUsing(CardUsingEventArgs args)
		{
			if (args.Card.IsBasic && args.ConsumingMana.Colorless > 0)
			{
				base.NotifyActivating();
			}
		}
	}
}
