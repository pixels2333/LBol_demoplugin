using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000484 RID: 1156
	[UsedImplicitly]
	public sealed class LunaticPassion : Card
	{
		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000F77 RID: 3959 RVA: 0x0001BA51 File Offset: 0x00019C51
		[UsedImplicitly]
		public int Percentage
		{
			get
			{
				return 150;
			}
		}

		// Token: 0x06000F78 RID: 3960 RVA: 0x0001BA58 File Offset: 0x00019C58
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<LunaticPassionSe>(0, 0, 0, 0, 0.2f);
			yield return base.BuffAction<MoodPassion>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
