using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class Bingxiang : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<StatusEffectApplyEventArgs>(base.Owner.StatusEffectAdding, new GameEventHandler<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}
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
