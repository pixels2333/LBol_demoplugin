using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x0200035F RID: 863
	public sealed class Frog : Card
	{
		// Token: 0x17000161 RID: 353
		// (get) Token: 0x06000C6E RID: 3182 RVA: 0x00018332 File Offset: 0x00016532
		[UsedImplicitly]
		public string CardName
		{
			get
			{
				if (this.OriginalCard == null)
				{
					return "Game.UnknownCard".Localize(true);
				}
				return this.OriginalCard.Name;
			}
		}

		// Token: 0x17000162 RID: 354
		// (get) Token: 0x06000C6F RID: 3183 RVA: 0x00018353 File Offset: 0x00016553
		// (set) Token: 0x06000C70 RID: 3184 RVA: 0x0001835B File Offset: 0x0001655B
		public Card OriginalCard { get; set; }

		// Token: 0x06000C71 RID: 3185 RVA: 0x00018364 File Offset: 0x00016564
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card originalCard = this.OriginalCard;
			if (originalCard != null && originalCard.Battle == null)
			{
				yield return new AddCardsToHandAction(new Card[] { this.OriginalCard });
			}
			yield break;
		}

		// Token: 0x06000C72 RID: 3186 RVA: 0x00018374 File Offset: 0x00016574
		public override IEnumerable<BattleAction> AfterUseAction()
		{
			yield return new RemoveCardAction(this);
			yield break;
		}

		// Token: 0x06000C73 RID: 3187 RVA: 0x00018384 File Offset: 0x00016584
		public override IEnumerable<BattleAction> AfterFollowPlayAction()
		{
			yield return new RemoveCardAction(this);
			yield break;
		}

		// Token: 0x06000C74 RID: 3188 RVA: 0x00018394 File Offset: 0x00016594
		public override IEnumerable<Card> EnumerateRelativeCards()
		{
			if (this.OriginalCard != null)
			{
				yield return this.OriginalCard;
			}
			yield break;
		}
	}
}
