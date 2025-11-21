using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200017A RID: 378
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9)]
	public sealed class JudaShaozi : Exhibit
	{
		// Token: 0x06000546 RID: 1350 RVA: 0x0000CFEF File Offset: 0x0000B1EF
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsAdded, delegate(CardsEventArgs args)
			{
				int num = Enumerable.Count<Card>(args.Cards, (Card card) => card.CardType == CardType.Misfortune);
				if (num > 0)
				{
					base.NotifyActivating();
					base.GameRun.GainMaxHp(num * base.Value1, true, true);
				}
			});
		}
	}
}
