using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001BC RID: 444
	[UsedImplicitly]
	public sealed class Dianshiji : Exhibit
	{
		// Token: 0x06000667 RID: 1639 RVA: 0x0000EBC9 File Offset: 0x0000CDC9
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}

		// Token: 0x06000668 RID: 1640 RVA: 0x0000EBE8 File Offset: 0x0000CDE8
		private void OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			if (args.Effect is Weak)
			{
				args.CancelBy(this);
				base.NotifyActivating();
			}
		}
	}
}
