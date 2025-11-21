using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.EnemyUnits.Normal.Guihuos
{
	// Token: 0x02000200 RID: 512
	[UsedImplicitly]
	public sealed class GuihuoRed : Guihuo
	{
		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x06000812 RID: 2066 RVA: 0x00011F15 File Offset: 0x00010115
		protected override Type DebuffType
		{
			get
			{
				return typeof(Vulnerable);
			}
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000813 RID: 2067 RVA: 0x00011F21 File Offset: 0x00010121
		protected override string SkillVFX
		{
			get
			{
				return "GuihuoRskill";
			}
		}
	}
}
