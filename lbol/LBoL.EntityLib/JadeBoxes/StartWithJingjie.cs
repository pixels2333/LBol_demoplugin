using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.EntityLib.Exhibits.Adventure;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class StartWithJingjie : JadeBox
	{
		protected override void OnGain(GameRunController gameRun)
		{
			gameRun.GainExhibitInstantly(Library.CreateExhibit<JingjieGanzhiyi>(), false, null);
		}
	}
}
