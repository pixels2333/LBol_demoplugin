using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002CF RID: 719
	[UsedImplicitly]
	public sealed class PrismriverRed : Card
	{
		// Token: 0x17000141 RID: 321
		// (get) Token: 0x06000AF7 RID: 2807 RVA: 0x00016611 File Offset: 0x00014811
		[UsedImplicitly]
		public int Light
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x06000AF8 RID: 2808 RVA: 0x00016614 File Offset: 0x00014814
		protected override string GetBaseDescription()
		{
			if (base.DebutActive || !this.IsUpgraded)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000AF9 RID: 2809 RVA: 0x00016633 File Offset: 0x00014833
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield return new DrawManyCardAction(base.Value1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Xingguang>(this.Light, false), AddCardsType.Normal);
			if (base.TriggeredAnyhow)
			{
				yield return new GainPowerAction(base.Value2);
			}
			yield break;
		}
	}
}
