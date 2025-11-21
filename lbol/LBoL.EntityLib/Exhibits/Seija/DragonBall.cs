using System;
using JetBrains.Annotations;
using LBoL.EntityLib.StatusEffects.Enemy.Seija;

namespace LBoL.EntityLib.Exhibits.Seija
{
	// Token: 0x02000145 RID: 325
	[UsedImplicitly]
	public sealed class DragonBall : SeijaExhibit
	{
		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000475 RID: 1141 RVA: 0x0000BCCF File Offset: 0x00009ECF
		protected override Type SeType
		{
			get
			{
				return typeof(DragonBallSe);
			}
		}
	}
}
