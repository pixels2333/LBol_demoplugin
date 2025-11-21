using System;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000416 RID: 1046
	[UsedImplicitly]
	public sealed class CollectionDefense : Card
	{
		// Token: 0x17000196 RID: 406
		// (get) Token: 0x06000E64 RID: 3684 RVA: 0x0001A6E4 File Offset: 0x000188E4
		protected override int AdditionalBlock
		{
			get
			{
				if (base.Battle == null || this.IsUpgraded)
				{
					return 0;
				}
				return base.Battle.DrawZone.Count;
			}
		}

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x06000E65 RID: 3685 RVA: 0x0001A708 File Offset: 0x00018908
		protected override int AdditionalShield
		{
			get
			{
				if (base.Battle == null || !this.IsUpgraded)
				{
					return 0;
				}
				return base.Battle.DrawZone.Count;
			}
		}
	}
}
