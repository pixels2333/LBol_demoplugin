using System;
using JetBrains.Annotations;
using LBoL.Core;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class PingzhuangLinghun : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<DieEventArgs>(base.Battle.EnemyPointGenerating, delegate(DieEventArgs args)
			{
				base.NotifyActivating();
				args.Money += base.Value1 * args.Power;
				args.Power = 0;
			});
		}
	}
}
