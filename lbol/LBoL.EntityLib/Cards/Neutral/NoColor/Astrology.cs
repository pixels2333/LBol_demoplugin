using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	// Token: 0x020002D7 RID: 727
	[UsedImplicitly]
	public sealed class Astrology : Card
	{
		// Token: 0x06000B0A RID: 2826 RVA: 0x00016729 File Offset: 0x00014929
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
