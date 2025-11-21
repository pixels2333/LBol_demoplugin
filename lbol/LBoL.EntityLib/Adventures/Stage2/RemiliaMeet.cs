using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoL.EntityLib.Exhibits.Common;
using Yarn;

namespace LBoL.EntityLib.Adventures.Stage2
{
	// Token: 0x02000509 RID: 1289
	public sealed class RemiliaMeet : Adventure
	{
		// Token: 0x060010F4 RID: 4340 RVA: 0x0001E7DA File Offset: 0x0001C9DA
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$hasExhibit", base.GameRun.Player.HasExhibit<Yuhangfu>());
		}

		// Token: 0x060010F5 RID: 4341 RVA: 0x0001E7F8 File Offset: 0x0001C9F8
		[RuntimeCommand("replace", "")]
		[UsedImplicitly]
		public void Replace()
		{
			List<Card> list = new List<Card>();
			List<Card> list2 = new List<Card>();
			foreach (Card card in base.GameRun.BaseDeck)
			{
				if (card.CardType == CardType.Attack && card.IsBasic)
				{
					list.Add(card);
					Card card2;
					if (card.Cost.Amount == 1)
					{
						card2 = LBoL.Core.Library.CreateCard<VampireShoot1>();
					}
					else
					{
						card2 = LBoL.Core.Library.CreateCard<VampireShoot2>();
					}
					if (card.IsUpgraded)
					{
						card2.Upgrade();
					}
					list2.Add(card2);
				}
			}
			base.GameRun.RemoveDeckCards(list, false);
			base.GameRun.AddDeckCards(list2, true, null);
		}

		// Token: 0x060010F6 RID: 4342 RVA: 0x0001E8BC File Offset: 0x0001CABC
		[RuntimeCommand("deal", "")]
		[UsedImplicitly]
		public void Deal()
		{
			base.GameRun.LoseExhibit(base.GameRun.Player.GetExhibit<Yuhangfu>(), false, true);
		}
	}
}
