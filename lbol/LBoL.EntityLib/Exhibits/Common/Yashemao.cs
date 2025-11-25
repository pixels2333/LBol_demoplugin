using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Yashemao : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.DrawCardCount += base.Value1;
			if (base.Battle != null)
			{
				base.Battle.DrawCardCount += base.Value1;
			}
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.DrawCardCount -= base.Value1;
			if (base.Battle != null)
			{
				base.Battle.DrawCardCount -= base.Value1;
			}
		}
	}
}
