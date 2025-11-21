using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.EnemyUnits.Normal.Guihuos
{
	// Token: 0x020001FF RID: 511
	[UsedImplicitly]
	public sealed class GuihuoGreen : Guihuo
	{
		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x0600080F RID: 2063 RVA: 0x00011EFA File Offset: 0x000100FA
		protected override Type DebuffType
		{
			get
			{
				return typeof(Fragil);
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x06000810 RID: 2064 RVA: 0x00011F06 File Offset: 0x00010106
		protected override string SkillVFX
		{
			get
			{
				return "GuihuoGskill";
			}
		}
	}
}
