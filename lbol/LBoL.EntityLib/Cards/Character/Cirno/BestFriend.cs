using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x02000498 RID: 1176
	[UsedImplicitly]
	public sealed class BestFriend : Card
	{
		// Token: 0x170001BA RID: 442
		// (get) Token: 0x06000FB8 RID: 4024 RVA: 0x0001C011 File Offset: 0x0001A211
		public override ManaGroup? PlentifulMana
		{
			get
			{
				return new ManaGroup?(base.Mana);
			}
		}

		// Token: 0x06000FB9 RID: 4025 RVA: 0x0001C01E File Offset: 0x0001A21E
		protected override string GetBaseDescription()
		{
			if (!base.PlentifulHappenThisTurn)
			{
				return base.GetBaseDescription();
			}
			return base.GetExtraDescription1;
		}

		// Token: 0x06000FBA RID: 4026 RVA: 0x0001C038 File Offset: 0x0001A238
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this && card.CardType == CardType.Friend));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, base.Value1, list);
		}

		// Token: 0x06000FBB RID: 4027 RVA: 0x0001C07F File Offset: 0x0001A27F
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			IReadOnlyList<Card> readOnlyList = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards : null);
			if (readOnlyList != null)
			{
				using (IEnumerator<Card> enumerator = readOnlyList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card = enumerator.Current;
						card.NotifyActivating();
						card.Loyalty += 9;
					}
					yield break;
				}
			}
			yield break;
		}
	}
}
