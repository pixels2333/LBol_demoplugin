using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200043A RID: 1082
	[UsedImplicitly]
	public sealed class PurpleMogu : Card
	{
		// Token: 0x06000EC7 RID: 3783 RVA: 0x0001AEA8 File Offset: 0x000190A8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.HealAction(base.Value1);
			yield break;
		}
	}
}
