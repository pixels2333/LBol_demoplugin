using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using UnityEngine;
using Yarn;
namespace LBoL.EntityLib.Adventures.FirstPlace
{
	[AdventureInfo(WeighterType = typeof(ShinmyoumaruForge.ShinmyoumaruForgeWeighter))]
	public sealed class ShinmyoumaruForge : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			bool flag = Enumerable.Count<Card>(base.GameRun.BaseDeck, (Card c) => c.CanUpgrade && c.IsBasic) > 0;
			storage.SetValue("$hasUpgradableBasics", flag);
			storage.SetValue("$upgradeCount", 5f);
			bool flag2 = Enumerable.Count<Card>(base.GameRun.BaseDeckWithoutUnremovable, (Card c) => !c.IsBasic) > 0;
			storage.SetValue("$hasNonBasics", flag2);
			storage.SetValue("$replaceCount", 3f);
			bool flag3 = Enumerable.Count<Card>(base.GameRun.BaseDeck, (Card c) => c.IsBasic) > 0;
			storage.SetValue("$hasBasics", flag3);
			int num = Mathf.FloorToInt((float)base.GameRun.Player.MaxHp * 0.2f);
			storage.SetValue("$loseMax", (float)num);
		}
		[RuntimeCommand("upgradeBasic", "")]
		[UsedImplicitly]
		public IEnumerator UpgradeBasic(string description)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.GameRun.BaseDeck, (Card c) => c.CanUpgrade && c.IsBasic));
			int num = Math.Min(list.Count, 5);
			if (num == 0)
			{
				yield break;
			}
			SelectCardInteraction interaction = new SelectCardInteraction(num, num, list, SelectedCardHandling.DoNothing)
			{
				CanCancel = false,
				Description = description
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
			base.GameRun.UpgradeDeckCards(selectedCards, true);
			yield return new WaitForSeconds(1f);
			yield break;
		}
		[RuntimeCommand("removeNonBasic", "")]
		[UsedImplicitly]
		public IEnumerator RemoveNonBasic(string description, bool canCancel = false)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.GameRun.BaseDeckWithoutUnremovable, (Card c) => !c.IsBasic));
			RemoveCardInteraction interaction = new RemoveCardInteraction(list)
			{
				CanCancel = canCancel,
				Description = description
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			if (!interaction.IsCanceled)
			{
				base.GameRun.RemoveDeckCards(new Card[] { interaction.SelectedCard }, true);
			}
			yield break;
		}
		[RuntimeCommand("replaceBasic", "")]
		[UsedImplicitly]
		public IEnumerator ReplaceBasic(string description)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.GameRun.BaseDeck, (Card c) => c.IsBasic));
			int num = Math.Min(list.Count, 3);
			if (num == 0)
			{
				yield break;
			}
			SelectCardInteraction interaction = new SelectCardInteraction(num, num, list, SelectedCardHandling.DoNothing)
			{
				CanCancel = false,
				Description = description
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
			if (selectedCards.Count > 0)
			{
				base.GameRun.RemoveDeckCards(selectedCards, false);
				List<Card> list2 = new List<Card>();
				using (IEnumerator<Card> enumerator = selectedCards.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card = enumerator.Current;
						Card card2 = base.GameRun.RollCard(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.OnlyCommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, (CardConfig config) => config.Type == card.CardType);
						if (card.IsUpgraded && card2.CanUpgrade)
						{
							card2.Upgrade();
						}
						list2.Add(card2);
					}
				}
				base.GameRun.AddDeckCards(list2, true, null);
				yield return new WaitForSeconds(2f);
			}
			yield break;
		}
		private const int UpgradeCount = 5;
		private const int ReplaceCount = 3;
		private const float LoseRatio = 0.2f;
		private class ShinmyoumaruForgeWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((Enumerable.Count<Card>(gameRun.BaseDeck, (Card c) => c.IsBasic) > 0) ? 1 : 0);
			}
		}
	}
}
