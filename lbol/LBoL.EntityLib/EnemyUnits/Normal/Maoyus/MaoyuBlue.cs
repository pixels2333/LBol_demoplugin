using System;
using JetBrains.Annotations;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.EnemyUnits.Normal.Maoyus
{
	// Token: 0x020001FA RID: 506
	[UsedImplicitly]
	public sealed class MaoyuBlue : MaoyuOrigin
	{
		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060007F4 RID: 2036 RVA: 0x00011A1F File Offset: 0x0000FC1F
		protected override Type DebuffType
		{
			get
			{
				return typeof(Weak);
			}
		}
	}
}
