using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000289 RID: 649
	[UsedImplicitly]
	public sealed class BailianMagic : Card
	{
		// Token: 0x06000A36 RID: 2614 RVA: 0x000156F5 File Offset: 0x000138F5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BailianMagicSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
