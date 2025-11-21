using System;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits
{
	// Token: 0x0200011D RID: 285
	public class MythicExhibit : ShiningExhibit
	{
		// Token: 0x060003EE RID: 1006 RVA: 0x0000AE21 File Offset: 0x00009021
		protected override void OnAdded(PlayerUnit player)
		{
			if (base.GameRun.Difficulty == GameDifficulty.Easy)
			{
				base.GameRun.TrueEndingBlockers.Add(this);
			}
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x0000AE42 File Offset: 0x00009042
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.TrueEndingBlockers.Remove(this);
		}
	}
}
