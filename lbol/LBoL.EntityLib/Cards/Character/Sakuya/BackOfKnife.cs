using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200037F RID: 895
	[UsedImplicitly]
	public sealed class BackOfKnife : Card
	{
		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000CC8 RID: 3272 RVA: 0x00018998 File Offset: 0x00016B98
		[UsedImplicitly]
		public int IncreasedDamage
		{
			get
			{
				if (base.Battle != null)
				{
					return Enumerable.Count<Card>(base.Battle.ExileZone, (Card c) => c is Knife) * base.Value1;
				}
				return 0;
			}
		}

		// Token: 0x06000CC9 RID: 3273 RVA: 0x000189E5 File Offset: 0x00016BE5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			GroupOfKnife groupOfKnife = Library.CreateCard<GroupOfKnife>();
			List<Card> knifes = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.ExileZone, (Card card) => card is Knife));
			groupOfKnife.DeltaDamage = this.IncreasedDamage;
			yield return new AddCardsToHandAction(new Card[] { groupOfKnife });
			foreach (Card card2 in knifes)
			{
				yield return new RemoveCardAction(card2);
			}
			List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			yield break;
			yield break;
		}
	}
}
