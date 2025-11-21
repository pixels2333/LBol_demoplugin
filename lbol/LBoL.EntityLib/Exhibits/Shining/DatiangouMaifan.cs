using System;
using JetBrains.Annotations;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000126 RID: 294
	[UsedImplicitly]
	public sealed class DatiangouMaifan : ShiningExhibit
	{
		// Token: 0x06000409 RID: 1033 RVA: 0x0000B0D4 File Offset: 0x000092D4
		protected override void OnLeaveBattle()
		{
			PlayerUnit owner = base.Owner;
			if (owner != null && owner.IsAlive)
			{
				base.NotifyActivating();
				base.GameRun.GainPower(base.Value1, true);
			}
		}
	}
}
