using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000410 RID: 1040
	[UsedImplicitly]
	public sealed class BlueMogu : Card
	{
		// Token: 0x06000E58 RID: 3672 RVA: 0x0001A654 File Offset: 0x00018854
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
