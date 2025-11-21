using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Black;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x0200032E RID: 814
	[UsedImplicitly]
	public sealed class DreamExpress : Card
	{
		// Token: 0x06000BED RID: 3053 RVA: 0x00017924 File Offset: 0x00015B24
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<DreamExpressSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
