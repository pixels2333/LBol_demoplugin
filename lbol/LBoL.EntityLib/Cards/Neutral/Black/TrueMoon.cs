using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.Black;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000341 RID: 833
	[UsedImplicitly]
	public sealed class TrueMoon : Card
	{
		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000C1C RID: 3100 RVA: 0x00017C5E File Offset: 0x00015E5E
		public override ManaGroup? PlentifulMana
		{
			get
			{
				return new ManaGroup?(base.Mana);
			}
		}

		// Token: 0x06000C1D RID: 3101 RVA: 0x00017C6B File Offset: 0x00015E6B
		protected override string GetBaseDescription()
		{
			if (!base.PlentifulHappenThisTurn)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000C1E RID: 3102 RVA: 0x00017C82 File Offset: 0x00015E82
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<UseCardToLoseGame>(0, 0, 0, base.Value2, 0.2f);
			yield break;
		}
	}
}
