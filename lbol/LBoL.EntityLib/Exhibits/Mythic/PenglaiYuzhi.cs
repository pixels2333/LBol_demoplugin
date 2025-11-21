using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;

namespace LBoL.EntityLib.Exhibits.Mythic
{
	// Token: 0x02000152 RID: 338
	[UsedImplicitly]
	public sealed class PenglaiYuzhi : MythicExhibit
	{
		// Token: 0x0600049A RID: 1178 RVA: 0x0000BF0B File Offset: 0x0000A10B
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<ManaEventArgs>(base.Battle.ManaGaining, new GameEventHandler<ManaEventArgs>(this.OnManaGaining));
			base.HandleBattleEvent<ManaEventArgs>(base.Battle.ManaLosing, new GameEventHandler<ManaEventArgs>(this.OnManaLosing));
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x0000BF48 File Offset: 0x0000A148
		private void OnManaGaining(ManaEventArgs args)
		{
			if (args.Value.WithPhilosophy(0) != ManaGroup.Empty)
			{
				base.NotifyActivating();
				args.Value = ManaGroup.Philosophies(args.Value.Amount);
				args.AddModifier(this);
			}
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x0000BF94 File Offset: 0x0000A194
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
