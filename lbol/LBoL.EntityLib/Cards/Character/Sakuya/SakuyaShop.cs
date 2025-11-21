using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003AE RID: 942
	[UsedImplicitly]
	public sealed class SakuyaShop : Card
	{
		// Token: 0x06000D5F RID: 3423 RVA: 0x00019422 File Offset: 0x00017622
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
