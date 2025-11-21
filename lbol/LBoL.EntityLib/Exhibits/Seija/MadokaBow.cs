using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x02000148 RID: 328
	[UsedImplicitly]
	public sealed class MadokaBow : SeijaExhibit
	{
		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600047C RID: 1148 RVA: 0x0000BD1F File Offset: 0x00009F1F
		protected override Type SeType
		{
			get
			{
				return typeof(MadokaBowSe);
			}
		}

		// Token: 0x0600047D RID: 1149 RVA: 0x0000BD2B File Offset: 0x00009F2B
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Level = 2;
		}
	}
}
