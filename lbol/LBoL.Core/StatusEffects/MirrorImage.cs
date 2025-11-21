using System;
using JetBrains.Annotations;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000A3 RID: 163
	[UsedImplicitly]
	public sealed class MirrorImage : StatusEffect
	{
		// Token: 0x17000278 RID: 632
		// (get) Token: 0x0600078F RID: 1935 RVA: 0x000164A8 File Offset: 0x000146A8
		public override string UnitEffectName
		{
			get
			{
				return "MirrorImage";
			}
		}

		// Token: 0x04000353 RID: 851
		public const float LootRate = 0.25f;
	}
}
