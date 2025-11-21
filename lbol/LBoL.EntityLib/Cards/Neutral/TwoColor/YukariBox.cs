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
	// Token: 0x020002BF RID: 703
	[UsedImplicitly]
	public sealed class YukariBox : Card
	{
		// Token: 0x06000AC5 RID: 2757 RVA: 0x000161EF File Offset: 0x000143EF
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			yield return new DrawManyCardAction(base.Value1);
			yield return new ReshuffleAction();
			yield break;
		}
	}
}
