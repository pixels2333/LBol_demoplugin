using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004F4 RID: 1268
	[UsedImplicitly]
	public sealed class ArtificialJewellery : Card
	{
		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x060010AF RID: 4271 RVA: 0x0001D2A0 File Offset: 0x0001B4A0
		public override bool UpgradeIsPositive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x060010B0 RID: 4272 RVA: 0x0001D2A3 File Offset: 0x0001B4A3
		public override bool Negative
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060010B1 RID: 4273 RVA: 0x0001D2A6 File Offset: 0x0001B4A6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (this.IsUpgraded)
			{
				yield return new GainManaAction(base.Mana);
			}
			else
			{
				ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(base.GameRun.BattleRng));
				yield return new GainManaAction(manaGroup);
			}
			yield break;
		}
	}
}
