using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class XianzheShi : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<ManaEventArgs>(base.Battle.ManaLosing, new GameEventHandler<ManaEventArgs>(this.OnManaLosing));
		}
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
