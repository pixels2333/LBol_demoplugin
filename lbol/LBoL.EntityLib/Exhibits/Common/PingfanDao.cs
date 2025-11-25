using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class PingfanDao : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.BasicAttackCardExtraDamage1 += base.Value1;
			base.GameRun.BasicAttackCardExtraDamage2 += base.Value2;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.BasicAttackCardExtraDamage1 -= base.Value1;
			base.GameRun.BasicAttackCardExtraDamage2 -= base.Value2;
		}
	}
}
