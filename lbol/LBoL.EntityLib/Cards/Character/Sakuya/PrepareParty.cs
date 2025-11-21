using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200039E RID: 926
	[UsedImplicitly]
	public sealed class PrepareParty : Card
	{
		// Token: 0x06000D37 RID: 3383 RVA: 0x00019158 File Offset: 0x00017358
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, 1, list);
			}
			return null;
		}

		// Token: 0x06000D38 RID: 3384 RVA: 0x00019199 File Offset: 0x00017399
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			Card card = ((selectHandInteraction != null) ? Enumerable.FirstOrDefault<Card>(selectHandInteraction.SelectedCards) : null);
			if (card != null)
			{
				yield return new MoveCardToDrawZoneAction(card, DrawZoneTarget.Top);
			}
			yield return base.DefenseAction(true);
			yield break;
		}
	}
}
