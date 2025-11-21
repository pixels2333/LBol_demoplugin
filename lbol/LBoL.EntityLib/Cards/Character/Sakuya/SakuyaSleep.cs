using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003AF RID: 943
	[UsedImplicitly]
	public sealed class SakuyaSleep : Card
	{
		// Token: 0x06000D61 RID: 3425 RVA: 0x0001943A File Offset: 0x0001763A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<WManaCard>() });
			if (!this.IsUpgraded)
			{
				yield return new RequestEndPlayerTurnAction();
			}
			yield break;
		}
	}
}
