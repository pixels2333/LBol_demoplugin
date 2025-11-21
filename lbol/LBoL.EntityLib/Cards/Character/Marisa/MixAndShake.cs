using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000431 RID: 1073
	[UsedImplicitly]
	public sealed class MixAndShake : Card
	{
		// Token: 0x1700019B RID: 411
		// (get) Token: 0x06000EAA RID: 3754 RVA: 0x0001AC33 File Offset: 0x00018E33
		protected override int AdditionalDamage
		{
			get
			{
				if (base.Battle != null)
				{
					return base.Value1 * (Enumerable.Count<Potion>(Enumerable.OfType<Potion>(base.Battle.DrawZone)) + Enumerable.Count<Potion>(Enumerable.OfType<Potion>(base.Battle.DiscardZone)));
				}
				return 0;
			}
		}
	}
}
