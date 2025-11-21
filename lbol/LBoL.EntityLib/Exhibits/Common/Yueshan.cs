using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001AE RID: 430
	[UsedImplicitly]
	public sealed class Yueshan : Exhibit
	{
		// Token: 0x0600062C RID: 1580 RVA: 0x0000E56B File Offset: 0x0000C76B
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<BlockShieldEventArgs>(base.Owner.BlockShieldLosing, new GameEventHandler<BlockShieldEventArgs>(this.OnBlockShieldLosing));
		}

		// Token: 0x0600062D RID: 1581 RVA: 0x0000E58A File Offset: 0x0000C78A
		private void OnBlockShieldLosing(BlockShieldEventArgs args)
		{
			if (args.Cause == ActionCause.TurnStart && args.Block > 0f)
			{
				args.Block = (float)(args.Block / 2f).FloorToInt();
				args.AddModifier(this);
				base.NotifyActivating();
			}
		}
	}
}
