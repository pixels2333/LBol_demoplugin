using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001BA RID: 442
	[UsedImplicitly]
	public sealed class Bingxiang : Exhibit
	{
		// Token: 0x0600065F RID: 1631 RVA: 0x0000EB04 File Offset: 0x0000CD04
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}

		// Token: 0x06000660 RID: 1632 RVA: 0x0000EB23 File Offset: 0x0000CD23
		private void OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			if (args.Effect is Fragil)
			{
				args.CancelBy(this);
				base.NotifyActivating();
			}
		}
	}
}
