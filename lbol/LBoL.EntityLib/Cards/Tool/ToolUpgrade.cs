using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Tool
{
	// Token: 0x02000268 RID: 616
	[UsedImplicitly]
	public sealed class ToolUpgrade : Card
	{
		// Token: 0x060009DE RID: 2526 RVA: 0x00015013 File Offset: 0x00013213
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.UpgradeAllHandsAction();
			yield break;
		}
	}
}
