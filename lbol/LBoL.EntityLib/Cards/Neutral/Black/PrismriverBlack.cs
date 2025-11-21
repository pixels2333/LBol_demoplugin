using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000337 RID: 823
	[UsedImplicitly]
	public sealed class PrismriverBlack : Card
	{
		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000C01 RID: 3073 RVA: 0x00017A77 File Offset: 0x00015C77
		[UsedImplicitly]
		public int Light
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x06000C02 RID: 3074 RVA: 0x00017A7A File Offset: 0x00015C7A
		protected override string GetBaseDescription()
		{
			if (base.DebutActive || !this.IsUpgraded)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000C03 RID: 3075 RVA: 0x00017A99 File Offset: 0x00015C99
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield return new DrawManyCardAction(base.Value1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<Yueguang>(this.Light, false), AddCardsType.Normal);
			if (base.TriggeredAnyhow)
			{
				yield return new GainPowerAction(base.Value2);
			}
			yield break;
		}
	}
}
