using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.EntityLib.Exhibits.Adventure;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x0200011B RID: 283
	[UsedImplicitly]
	public sealed class StartWithJingjie : JadeBox
	{
		// Token: 0x060003E9 RID: 1001 RVA: 0x0000ADF0 File Offset: 0x00008FF0
		protected override void OnGain(GameRunController gameRun)
		{
			gameRun.GainExhibitInstantly(Library.CreateExhibit<JingjieGanzhiyi>(), false, null);
		}
	}
}
