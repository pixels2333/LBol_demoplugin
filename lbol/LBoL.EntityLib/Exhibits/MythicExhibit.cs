using System;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits
{
	public class MythicExhibit : ShiningExhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			if (base.GameRun.Difficulty == GameDifficulty.Easy)
			{
				base.GameRun.TrueEndingBlockers.Add(this);
			}
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.TrueEndingBlockers.Remove(this);
		}
	}
}
