using System;
using System.Collections;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.EntityLib.Cards.Adventure;
using UnityEngine;
using Yarn;
namespace LBoL.EntityLib.Adventures.Shared23
{
	public sealed class NarumiOfferCard : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$maxhpCommon", 1f);
			storage.SetValue("$maxhpUncommon", 5f);
			storage.SetValue("$maxhpRare", 10f);
			storage.SetValue("$healPercentageUncommon", 30f);
		}
		[RuntimeCommand("offerDeckCard", "")]
		[UsedImplicitly]
		public IEnumerator OfferDeckCard(string description)
		{
			RemoveCardInteraction interaction = new RemoveCardInteraction(base.GameRun.BaseDeckWithoutUnremovable)
			{
				CanCancel = false,
				Description = description
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			Card selectedCard = interaction.SelectedCard;
			base.GameRun.RemoveDeckCards(new Card[] { selectedCard }, true);
			if (selectedCard is ArtificialJewellery && base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>())
			{
				base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.NarumiAdventure);
			}
			if (interaction.SelectedCard.CardType == CardType.Misfortune)
			{
				base.Storage.SetValue("$target", "Misfortune");
			}
			else
			{
				DialogStorage storage = base.Storage;
				string text;
				switch (interaction.SelectedCard.Config.Rarity)
				{
				case Rarity.Common:
					text = "Common";
					break;
				case Rarity.Uncommon:
					text = "Uncommon";
					break;
				case Rarity.Rare:
					text = "Rare";
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				storage.SetValue("$target", text);
			}
			yield break;
		}
		[RuntimeCommand("cotinueCommon", "")]
		[UsedImplicitly]
		public void CotinueCommon()
		{
			base.GameRun.GainMaxHp(1, true, true);
		}
		[RuntimeCommand("cotinueUncommon", "")]
		[UsedImplicitly]
		public void CotinueUncommon()
		{
			base.GameRun.GainMaxHp(5, false, true);
			base.GameRun.Heal(Mathf.RoundToInt((float)(base.GameRun.Player.MaxHp * 30) / 100f), true, null);
		}
		[RuntimeCommand("cotinueRare", "")]
		[UsedImplicitly]
		public void CotinueRare()
		{
			base.GameRun.GainMaxHp(10, false, true);
			base.GameRun.HealToMaxHp(true, null);
		}
		private const int MaxhpCommon = 1;
		private const int MaxhpUncommon = 5;
		private const int MaxhpRare = 10;
		private const int HealPercentageUncommon = 30;
	}
}
