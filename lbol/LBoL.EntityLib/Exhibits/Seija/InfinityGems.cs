using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x02000147 RID: 327
	[UsedImplicitly]
	public sealed class InfinityGems : SeijaExhibit
	{
		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000479 RID: 1145 RVA: 0x0000BCF7 File Offset: 0x00009EF7
		protected override Type SeType
		{
			get
			{
				return typeof(InfinityGemsSe);
			}
		}

		// Token: 0x0600047A RID: 1146 RVA: 0x0000BD03 File Offset: 0x00009F03
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Count = 2;
		}
	}
}
