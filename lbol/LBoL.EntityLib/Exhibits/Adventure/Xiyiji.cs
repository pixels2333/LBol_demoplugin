using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001CB RID: 459
	[UsedImplicitly]
	public sealed class Xiyiji : Exhibit
	{
		// Token: 0x060006A4 RID: 1700 RVA: 0x0000F1D7 File Offset: 0x0000D3D7
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x0000F1F6 File Offset: 0x0000D3F6
		private void OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			if (args.Effect is Vulnerable)
			{
				args.CancelBy(this);
				base.NotifyActivating();
			}
		}
	}
}
