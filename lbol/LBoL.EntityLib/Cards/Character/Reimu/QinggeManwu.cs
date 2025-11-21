using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003E6 RID: 998
	[UsedImplicitly]
	public sealed class QinggeManwu : Card
	{
		// Token: 0x06000DF6 RID: 3574 RVA: 0x00019F3D File Offset: 0x0001813D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> upgraded = null;
			DrawManyCardAction drawAction = new DrawManyCardAction(base.Value1);
			yield return drawAction;
			IReadOnlyList<Card> drawnCards = drawAction.DrawnCards;
			if (drawnCards != null && drawnCards.Count > 0)
			{
				upgraded = Enumerable.ToList<Card>(Enumerable.Where<Card>(drawnCards, (Card card) => card.IsUpgraded));
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(drawnCards, (Card card) => card.CanUpgradeAndPositive));
				if (list.Count > 0)
				{
					yield return new UpgradeCardsAction(list);
				}
			}
			if (base.Value2 > 0 && upgraded != null && upgraded.Count > 0)
			{
				foreach (Card card2 in upgraded)
				{
					card2.NotifyActivating();
				}
				yield return base.DefenseAction(0, base.Value2 * upgraded.Count, BlockShieldType.Direct, false);
			}
			yield break;
		}
	}
}
