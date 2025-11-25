using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
namespace LBoL.EntityLib.Exhibits.Mythic
{
	[UsedImplicitly]
	public sealed class PenglaiYuzhi : MythicExhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<ManaEventArgs>(base.Battle.ManaGaining, new GameEventHandler<ManaEventArgs>(this.OnManaGaining));
			base.HandleBattleEvent<ManaEventArgs>(base.Battle.ManaLosing, new GameEventHandler<ManaEventArgs>(this.OnManaLosing));
		}
		private void OnManaGaining(ManaEventArgs args)
		{
			if (args.Value.WithPhilosophy(0) != ManaGroup.Empty)
			{
				base.NotifyActivating();
				args.Value = ManaGroup.Philosophies(args.Value.Amount);
				args.AddModifier(this);
			}
		}
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
