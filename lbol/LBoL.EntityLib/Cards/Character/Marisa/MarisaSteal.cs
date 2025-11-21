using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x0200042F RID: 1071
	[UsedImplicitly]
	public sealed class MarisaSteal : Card
	{
		// Token: 0x06000EA4 RID: 3748 RVA: 0x0001ABA2 File Offset: 0x00018DA2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToHandAction(Library.CreateCards<PManaCard>(base.Value1, false), AddCardsType.Normal);
			if (base.Value2 > 0)
			{
				yield return new LockRandomTurnManaAction(base.Value2);
			}
			yield break;
		}
	}
}
