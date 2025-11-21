using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004AE RID: 1198
	[UsedImplicitly]
	public sealed class FairyAllOut : Card
	{
		// Token: 0x06000FEC RID: 4076 RVA: 0x0001C481 File Offset: 0x0001A681
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.CardType == CardType.Friend && card.Summoned));
			foreach (Card card2 in list)
			{
				IEnumerable<BattleAction> passiveActions = card2.GetPassiveActions();
				if (passiveActions != null)
				{
					foreach (BattleAction battleAction in passiveActions)
					{
						yield return battleAction;
					}
					IEnumerator<BattleAction> enumerator2 = null;
				}
			}
			List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			yield break;
			yield break;
		}
	}
}
