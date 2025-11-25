using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
using LBoL.EntityLib.PlayerUnits;
using Yarn;
namespace LBoL.EntityLib.Adventures.Stage3
{
	public sealed class SatoriCounseling : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			EnemyUnit enemyUnit = Enumerable.FirstOrDefault<EnemyUnit>(Enumerable.First<Stage>(base.GameRun.Stages).Boss.Generate(base.GameRun), (EnemyUnit enemy) => enemy.Config.Type == EnemyType.Boss);
			if (enemyUnit != null)
			{
				base.Storage.SetValue("$opponentName", enemyUnit.ShortNameWithColor);
			}
		}
		[RuntimeCommand("library", "")]
		[UsedImplicitly]
		public IEnumerator Library(string description)
		{
			Card[] array = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(new RarityWeightTable(1f, 0.8f, 0f, 0f), OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), 20, false, false, null);
			if (base.GameRun.Player is Koishi)
			{
				foreach (Card card in array)
				{
					if (card.CanUpgrade)
					{
						card.Upgrade();
					}
				}
			}
			else
			{
				base.GameRun.UpgradeNewDeckCardOnFlags(array);
			}
			SelectCardInteraction interaction = new SelectCardInteraction(1, 1, array, SelectedCardHandling.DoNothing)
			{
				CanCancel = false,
				Description = description
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			Card card2 = interaction.SelectedCards[0];
			base.Storage.SetValue("$pill", card2.Id);
			this._payedPill = card2;
			yield break;
		}
		[RuntimeCommand("analyse", "")]
		[UsedImplicitly]
		public IEnumerator Analyse(bool isPlayer, bool isColorful, string description)
		{
			List<Card> list = new List<Card>();
			GameRunController gameRun = base.GameRun;
			RandomGen adventureRng = base.GameRun.AdventureRng;
			CardWeightTable cardWeightTable = new CardWeightTable(RarityWeightTable.OnlyUncommon, new OwnerWeightTable((float)(isPlayer ? 1 : 0), (float)(isPlayer ? 0 : 1), 0f, (float)(isPlayer ? 0 : 1)), CardTypeWeightTable.CanBeLoot, false);
			int num = 3;
			bool flag = false;
			bool flag2 = false;
			Predicate<CardConfig> predicate;
			if (!isColorful)
			{
				predicate = (CardConfig config) => config.Colors.Count <= 1;
			}
			else
			{
				predicate = (CardConfig config) => config.Colors.Count > 1;
			}
			Card[] array = gameRun.RollCards(adventureRng, cardWeightTable, num, flag, flag2, predicate);
			list.AddRange(array);
			if (array.Length < 3)
			{
				base.Storage.SetValue("$noPill", true);
				Card[] array2 = base.GameRun.RollCards(base.GameRun.AdventureRng, new CardWeightTable(RarityWeightTable.OnlyUncommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), 3 - array.Length, false, false, null);
				list.AddRange(array2);
			}
			foreach (Card card in list)
			{
				card.Upgrade();
			}
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(list, true, true, false)
			{
				Description = description
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			Card selectedCard = interaction.SelectedCard;
			base.Storage.SetValue("$pill", selectedCard.Id);
			this._payedPill = selectedCard;
			yield break;
		}
		[RuntimeCommand("treat", "")]
		[UsedImplicitly]
		public void Treat()
		{
			base.GameRun.AddDeckCard(this._payedPill, true, null);
		}
		private Card _payedPill;
	}
}
