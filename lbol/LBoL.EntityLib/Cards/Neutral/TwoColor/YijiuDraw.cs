using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002BA RID: 698
	[UsedImplicitly]
	public sealed class YijiuDraw : Card
	{
		// Token: 0x06000AB7 RID: 2743 RVA: 0x000160E9 File Offset: 0x000142E9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
