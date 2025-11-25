using System;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	public sealed class JingjieGanzhiyi : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.GameRun.TrueEndingProviders.Add(this);
			if (player.HasExhibit<WaijieYanshuang>())
			{
				WaijieYanshuang exhibit = player.GetExhibit<WaijieYanshuang>();
				int counter = exhibit.Counter;
				exhibit.Counter = counter + 1;
			}
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.TrueEndingProviders.Remove(this);
		}
	}
}
