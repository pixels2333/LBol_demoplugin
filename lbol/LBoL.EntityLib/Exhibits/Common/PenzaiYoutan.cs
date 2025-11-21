using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000183 RID: 387
	[UsedImplicitly]
	public sealed class PenzaiYoutan : Exhibit
	{
		// Token: 0x06000570 RID: 1392 RVA: 0x0000D49A File Offset: 0x0000B69A
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<ManaEventArgs>(base.Battle.ManaLosing, new GameEventHandler<ManaEventArgs>(this.OnManaLosing));
		}

		// Token: 0x06000571 RID: 1393 RVA: 0x0000D4BC File Offset: 0x0000B6BC
		private void OnManaLosing(ManaEventArgs args)
		{
			if (args.Cause == ActionCause.TurnEnd && !args.Value.IsEmpty)
			{
				base.NotifyActivating();
				args.CancelBy(this);
			}
		}
	}
}
