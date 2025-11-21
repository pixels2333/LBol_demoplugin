using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x0200025E RID: 606
	[UsedImplicitly]
	public sealed class ToolDraw : Card
	{
		// Token: 0x060009C8 RID: 2504 RVA: 0x00014ED3 File Offset: 0x000130D3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
