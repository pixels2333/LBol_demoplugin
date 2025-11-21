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
	// Token: 0x02000517 RID: 1303
	public sealed class NarumiOfferCard : Adventure
	{
		// Token: 0x06001114 RID: 4372 RVA: 0x0001EF50 File Offset: 0x0001D150
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$maxhpCommon", 1f);
			storage.SetValue("$maxhpUncommon", 5f);
			storage.SetValue("$maxhpRare", 10f);
			storage.SetValue("$healPercentageUncommon", 30f);
		}

		// Token: 0x06001115 RID: 4373 RVA: 0x0001EF9D File Offset: 0x0001D19D
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

		// Token: 0x06001116 RID: 4374 RVA: 0x0001EFB3 File Offset: 0x0001D1B3
		[RuntimeCommand("cotinueCommon", "")]
		[UsedImplicitly]
		public void CotinueCommon()
		{
			base.GameRun.GainMaxHp(1, true, true);
		}

		// Token: 0x06001117 RID: 4375 RVA: 0x0001EFC3 File Offset: 0x0001D1C3
		[RuntimeCommand("cotinueUncommon", "")]
		[UsedImplicitly]
		public void CotinueUncommon()
		{
			base.GameRun.GainMaxHp(5, false, true);
			base.GameRun.Heal(Mathf.RoundToInt((float)(base.GameRun.Player.MaxHp * 30) / 100f), true, null);
		}

		// Token: 0x06001118 RID: 4376 RVA: 0x0001EFFF File Offset: 0x0001D1FF
		[RuntimeCommand("cotinueRare", "")]
		[UsedImplicitly]
		public void CotinueRare()
		{
			base.GameRun.GainMaxHp(10, false, true);
			base.GameRun.HealToMaxHp(true, null);
		}

		// Token: 0x0400012F RID: 303
		private const int MaxhpCommon = 1;

		// Token: 0x04000130 RID: 304
		private const int MaxhpUncommon = 5;

		// Token: 0x04000131 RID: 305
		private const int MaxhpRare = 10;

		// Token: 0x04000132 RID: 306
		private const int HealPercentageUncommon = 30;
	}
}
