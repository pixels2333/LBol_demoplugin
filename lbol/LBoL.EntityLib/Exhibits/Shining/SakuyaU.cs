using System;
using JetBrains.Annotations;
using LBoL.Core;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200013B RID: 315
	[UsedImplicitly]
	public sealed class SakuyaU : ShiningExhibit
	{
		// Token: 0x06000452 RID: 1106 RVA: 0x0000B844 File Offset: 0x00009A44
		protected override void OnEnterBattle()
		{
			base.Counter = base.Value2;
			base.Battle.DrawCardCount += base.Value1;
			base.HandleBattleEvent<UnitEventArgs>(base.Owner.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarting));
			base.HandleBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x0000B8AF File Offset: 0x00009AAF
		private void OnPlayerTurnStarting(UnitEventArgs args)
		{
			if (base.Counter == 0 && base.Battle.Player.IsExtraTurn)
			{
				base.Battle.DrawCardCount += base.Value1;
			}
		}

		// Token: 0x06000454 RID: 1108 RVA: 0x0000B8E4 File Offset: 0x00009AE4
		private void OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Counter > 0 && !base.Battle.Player.IsExtraTurn)
			{
				int num = base.Counter - 1;
				base.Counter = num;
				if (base.Counter == 0)
				{
					base.Battle.DrawCardCount -= base.Value1;
					return;
				}
			}
			else if (base.Counter == 0 && base.Battle.Player.IsExtraTurn)
			{
				base.Battle.DrawCardCount -= base.Value1;
			}
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x0000B96F File Offset: 0x00009B6F
		protected override void OnLeaveBattle()
		{
			if (base.Counter > 0)
			{
				base.Counter = 0;
				base.Battle.DrawCardCount -= base.Value1;
			}
		}
	}
}
