using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x0200014C RID: 332
	[UsedImplicitly]
	public sealed class Shendeng : SeijaExhibit
	{
		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000489 RID: 1161 RVA: 0x0000BDEE File Offset: 0x00009FEE
		protected override Type SeType
		{
			get
			{
				return typeof(ShendengSe);
			}
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x0000BDFA File Offset: 0x00009FFA
		protected override void GenerateEffect()
		{
			base.GenerateEffect();
			this._effect.Level = 3;
		}
	}
}
