using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class PenzaiYoutan : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<ManaEventArgs>(base.Battle.ManaLosing, new GameEventHandler<ManaEventArgs>(this.OnManaLosing));
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
