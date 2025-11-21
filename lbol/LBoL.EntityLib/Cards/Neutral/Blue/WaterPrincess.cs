using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000324 RID: 804
	[UsedImplicitly]
	public sealed class WaterPrincess : Card
	{
		// Token: 0x17000156 RID: 342
		// (get) Token: 0x06000BD3 RID: 3027 RVA: 0x00017747 File Offset: 0x00015947
		[UsedImplicitly]
		public int Special
		{
			get
			{
				if (base.Battle != null)
				{
					return base.Value1 * Enumerable.Count<Card>(base.Battle.HandZone, (Card card) => card != this);
				}
				return 0;
			}
		}

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x06000BD4 RID: 3028 RVA: 0x00017776 File Offset: 0x00015976
		protected override int AdditionalShield
		{
			get
			{
				return this.Special;
			}
		}

		// Token: 0x06000BD5 RID: 3029 RVA: 0x0001777E File Offset: 0x0001597E
		protected override string GetBaseDescription()
		{
			if (!base.DebutActive)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}

		// Token: 0x06000BD6 RID: 3030 RVA: 0x00017795 File Offset: 0x00015995
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.TriggeredAnyhow)
			{
				yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<UManaCard>() });
			}
			yield break;
		}
	}
}
