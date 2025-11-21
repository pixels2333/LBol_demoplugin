using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003D5 RID: 981
	[UsedImplicitly]
	public sealed class GapLady : Card
	{
		// Token: 0x06000DC8 RID: 3528 RVA: 0x00019B85 File Offset: 0x00017D85
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value1);
			yield return new GainManaAction(base.Mana);
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
