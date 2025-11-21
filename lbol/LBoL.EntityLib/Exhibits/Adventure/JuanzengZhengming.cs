using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C3 RID: 451
	[UsedImplicitly]
	public sealed class JuanzengZhengming : Exhibit
	{
		// Token: 0x06000688 RID: 1672 RVA: 0x0000EF17 File Offset: 0x0000D117
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.GainPower(base.Value2, false);
		}

		// Token: 0x06000689 RID: 1673 RVA: 0x0000EF2B File Offset: 0x0000D12B
		protected override void OnAdded(PlayerUnit player)
		{
			player.Us.MaxPowerLevel += base.Value1;
			player.Us.UsRepeatableType = UsRepeatableType.FreeToUse;
		}
	}
}
