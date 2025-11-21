using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000489 RID: 1161
	[UsedImplicitly]
	public sealed class PassionDefense : Card
	{
		// Token: 0x170001AF RID: 431
		// (get) Token: 0x06000F86 RID: 3974 RVA: 0x0001BB8D File Offset: 0x00019D8D
		public override bool Triggered
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodPassion>();
			}
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x06000F87 RID: 3975 RVA: 0x0001BBA9 File Offset: 0x00019DA9
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle == null || !base.Battle.Player.HasStatusEffect<MoodPassion>())
				{
					return 0;
				}
				return base.Value1;
			}
		}
	}
}
