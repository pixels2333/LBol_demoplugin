using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x0200014D RID: 333
	[UsedImplicitly]
	public sealed class SihunYu : SeijaExhibit
	{
		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600048C RID: 1164 RVA: 0x0000BE16 File Offset: 0x0000A016
		protected override Type SeType
		{
			get
			{
				return typeof(SihunYuSe);
			}
		}
	}
}
