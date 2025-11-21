using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x0200014A RID: 330
	[UsedImplicitly]
	public sealed class SakuraWand : SeijaExhibit
	{
		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000482 RID: 1154 RVA: 0x0000BD7C File Offset: 0x00009F7C
		protected override Type SeType
		{
			get
			{
				return typeof(SakuraWandSe);
			}
		}
	}
}
