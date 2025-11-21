using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Marisa;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000439 RID: 1081
	[UsedImplicitly]
	public sealed class PotionJungle : Card
	{
		// Token: 0x06000EC5 RID: 3781 RVA: 0x0001AE90 File Offset: 0x00019090
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<PotionJungleSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
