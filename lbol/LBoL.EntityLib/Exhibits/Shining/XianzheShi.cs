using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000141 RID: 321
	[UsedImplicitly]
	public sealed class XianzheShi : ShiningExhibit
	{
		// Token: 0x06000468 RID: 1128 RVA: 0x0000BB41 File Offset: 0x00009D41
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<ManaEventArgs>(base.Battle.ManaLosing, new GameEventHandler<ManaEventArgs>(this.OnManaLosing));
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x0000BB60 File Offset: 0x00009D60
		private void OnManaLosing(ManaEventArgs args)
		{
			if (args.Cause == ActionCause.TurnEnd && args.Value.Philosophy > 0)
			{
				base.NotifyActivating();
				args.Value -= ManaGroup.Philosophies(args.Value.Philosophy);
				args.AddModifier(this);
			}
		}
	}
}
