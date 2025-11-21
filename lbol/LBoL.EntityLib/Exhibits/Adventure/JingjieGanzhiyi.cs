using System;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C1 RID: 449
	public sealed class JingjieGanzhiyi : Exhibit
	{
		// Token: 0x0600067C RID: 1660 RVA: 0x0000EE20 File Offset: 0x0000D020
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

		// Token: 0x0600067D RID: 1661 RVA: 0x0000EE5C File Offset: 0x0000D05C
		protected override void OnRemoved(PlayerUnit player)
		{
			base.GameRun.TrueEndingProviders.Remove(this);
		}
	}
}
