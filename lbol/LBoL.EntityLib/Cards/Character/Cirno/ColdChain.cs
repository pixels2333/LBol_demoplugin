using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004A8 RID: 1192
	[UsedImplicitly]
	public sealed class ColdChain : Card
	{
		// Token: 0x06000FDC RID: 4060 RVA: 0x0001C324 File Offset: 0x0001A524
		public override Interaction Precondition()
		{
			IReadOnlyList<Card> drawZoneToShow = base.Battle.DrawZoneToShow;
			if (drawZoneToShow.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(0, base.Value1, drawZoneToShow, SelectedCardHandling.DoNothing);
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x0001C356 File Offset: 0x0001A556
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			IReadOnlyList<Card> readOnlyList = ((selectCardInteraction != null) ? selectCardInteraction.SelectedCards : null);
			if (readOnlyList != null && readOnlyList.Count > 0)
			{
				foreach (Card card in readOnlyList)
				{
					yield return new MoveCardAction(card, CardZone.Hand);
				}
				IEnumerator<Card> enumerator = null;
			}
			yield break;
			yield break;
		}
	}
}
