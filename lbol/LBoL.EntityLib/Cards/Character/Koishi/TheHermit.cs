using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000495 RID: 1173
	[UsedImplicitly]
	public sealed class TheHermit : Card
	{
		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06000FAD RID: 4013 RVA: 0x0001BF39 File Offset: 0x0001A139
		public override bool Triggered
		{
			get
			{
				return this.IsForceCost;
			}
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x06000FAE RID: 4014 RVA: 0x0001BF41 File Offset: 0x0001A141
		public override bool IsForceCost
		{
			get
			{
				return base.Battle != null && base.Battle.Player.HasStatusEffect<MoodEpiphany>();
			}
		}

		// Token: 0x06000FAF RID: 4015 RVA: 0x0001BF5D File Offset: 0x0001A15D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			if (this.IsUpgraded)
			{
				Card card = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone);
				if (card != null && card.CanUpgradeAndPositive)
				{
					yield return new UpgradeCardAction(card);
				}
			}
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
