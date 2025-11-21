using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x02000149 RID: 329
	[UsedImplicitly]
	public sealed class QiannianShenqi : SeijaExhibit
	{
		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600047F RID: 1151 RVA: 0x0000BD47 File Offset: 0x00009F47
		protected override Type SeType
		{
			get
			{
				return typeof(QiannianShenqiSe);
			}
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x0000BD53 File Offset: 0x00009F53
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Level = 2;
			this._effect.Limit = 10;
		}
	}
}
