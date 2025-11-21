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
	// Token: 0x0200043D RID: 1085
	[UsedImplicitly]
	public sealed class RedGiantStar : Card
	{
		// Token: 0x06000ED0 RID: 3792 RVA: 0x0001AF75 File Offset: 0x00019175
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Overdrive(base.Value2))
			{
				yield return base.OverdriveAction(base.Value2);
				yield return new AddCardsToHandAction(new Card[] { this.IsUpgraded ? Library.CreateCard<PManaCard>() : Library.CreateCard<RManaCard>() });
			}
			yield break;
		}
	}
}
