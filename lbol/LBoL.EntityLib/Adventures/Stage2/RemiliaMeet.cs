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
	public sealed class RemiliaMeet : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$hasExhibit", base.GameRun.Player.HasExhibit<Yuhangfu>());
		}
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
		[RuntimeCommand("deal", "")]
		[UsedImplicitly]
		public void Deal()
		{
			base.GameRun.LoseExhibit(base.GameRun.Player.GetExhibit<Yuhangfu>(), false, true);
		}
	}
}
