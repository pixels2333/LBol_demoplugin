using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200015E RID: 350
	public class AddCardsToHandAction : EventBattleAction<CardsEventArgs>
	{
		// Token: 0x170004D5 RID: 1237
		// (get) Token: 0x06000DC6 RID: 3526 RVA: 0x00025DB5 File Offset: 0x00023FB5
		public AddCardsType PresentationType { get; }

		// Token: 0x06000DC7 RID: 3527 RVA: 0x00025DBD File Offset: 0x00023FBD
		public AddCardsToHandAction(params Card[] cards)
			: this(cards, AddCardsType.Normal)
		{
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x00025DC7 File Offset: 0x00023FC7
		public AddCardsToHandAction(IEnumerable<Card> cards, AddCardsType presentationType = AddCardsType.Normal)
		{
			base.Args = new CardsEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards)
			};
			this.PresentationType = presentationType;
		}

		// Token: 0x06000DC9 RID: 3529 RVA: 0x00025DED File Offset: 0x00023FED
		internal override IEnumerable<Phase> GetPhases()
		{
			for (;;)
			{
				if (Enumerable.Count<Card>(base.Battle.HandZone, (Card card) => card.IsAutoExile) <= 0 || base.Battle.HandZone.Count + base.Args.Cards.Length - base.Battle.MaxHand <= 0)
				{
					break;
				}
				List<Card> autoExiles = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.IsAutoExile));
				int count = base.Battle.HandZone.Count + base.Args.Cards.Length - base.Battle.MaxHand;
				if (autoExiles.Count > 0 && count > 0)
				{
					yield return base.CreatePhase("AutoExile", delegate
					{
						this.Battle.React(new ExileManyCardAction(autoExiles.GetRange(0, Math.Min(autoExiles.Count, count))), Enumerable.First<Card>(this.Args.Cards), ActionCause.AutoExile);
					}, false);
				}
			}
			yield return base.CreatePhase("PreEvent", delegate
			{
				base.Battle.CardsAddingToHand.Execute(base.Args);
			}, false);
			yield return base.CreatePhase("Main", delegate
			{
				List<Card> list = new List<Card>();
				foreach (Card card in base.Args.Cards)
				{
					if (base.Battle.AddCardToHand(card) == CancelCause.None)
					{
						list.Add(card);
					}
				}
				if (!Enumerable.SequenceEqual<Card>(list, base.Args.Cards))
				{
					base.Args.Cards = list.ToArray();
					base.Args.IsModified = true;
				}
				base.Args.CanCancel = false;
			}, true);
			yield return base.CreatePhase("PostEvent", delegate
			{
				base.Battle.CardsAddedToHand.Execute(base.Args);
			}, false);
			yield break;
		}
	}
}
