using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x0200014E RID: 334
	[UsedImplicitly]
	public sealed class SingleJiandao : SeijaExhibit
	{
		// Token: 0x17000073 RID: 115
		// (get) Token: 0x0600048E RID: 1166 RVA: 0x0000BE2A File Offset: 0x0000A02A
		protected override Type SeType
		{
			get
			{
				return typeof(SingleJiandaoSe);
			}
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x0000BE36 File Offset: 0x0000A036
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Level = 1;
		}
	}
}
