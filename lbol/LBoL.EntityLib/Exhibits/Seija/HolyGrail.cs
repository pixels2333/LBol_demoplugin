using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x02000146 RID: 326
	[UsedImplicitly]
	public sealed class HolyGrail : SeijaExhibit
	{
		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000477 RID: 1143 RVA: 0x0000BCE3 File Offset: 0x00009EE3
		protected override Type SeType
		{
			get
			{
				return typeof(HolyGrailSe);
			}
		}
	}
}
