using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Yueshan : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<BlockShieldEventArgs>(base.Owner.BlockShieldLosing, new GameEventHandler<BlockShieldEventArgs>(this.OnBlockShieldLosing));
		}
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
