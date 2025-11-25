using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Exhibits.Adventure;
using Yarn;
namespace LBoL.EntityLib.Adventures
{
	public sealed class SumirekoGathering : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			IReadOnlyList<Card> baseDeck = base.GameRun.BaseDeck;
			this._rareCard = Enumerable.Where<Card>(baseDeck, (Card e) => e.Config.Rarity == Rarity.Rare && !e.Unremovable && e.CardType != CardType.Misfortune && e.CardType != CardType.Status).SampleOrDefault(base.GameRun.AdventureRng);
			if (this._rareCard != null)
			{
				this._hasRare = true;
				storage.SetValue("$isUpgraded", this._rareCard.IsUpgraded);
			}
			storage.SetValue("$hasRare", this._hasRare);
			CardWeightTable cardWeightTable = new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false);
			Card[] array = base.GameRun.RollCards(base.GameRun.AdventureRng, cardWeightTable, 3, false, false, (this._rareCard != null) ? ((CardConfig config) => config.Id != this._rareCard.Id) : null);
			storage.SetValue("$rareTrade1", array[0].Id);
			storage.SetValue("$rareTrade2", array[1].Id);
			storage.SetValue("$rareTrade3", array[2].Id);
			if (this._rareCard != null)
			{
				storage.SetValue("$canExchange1", true);
				storage.SetValue("$rareCard1", this._rareCard.Id);
				storage.SetValue("$rareCard1Name", this._rareCard.Name);
				return;
			}
			storage.SetValue("$canExchange1", false);
		}
		[RuntimeCommand("exchange", "")]
		[UsedImplicitly]
		public void Exchange()
		{
			base.GameRun.RemoveDeckCards(new Card[] { this._rareCard }, false);
		}
		[RuntimeCommand("trade", "")]
		[UsedImplicitly]
		public IEnumerator Trade(int index)
		{
			base.LoseExhibit("WaijieYouxiji");
			yield return base.GameRun.GainExhibitRunner(LBoL.Core.Library.CreateExhibit<WaijieYanshuang>(), true, new VisualSourceData
			{
				SourceType = VisualSourceType.Vn,
				Index = index
			});
			yield break;
		}
		private Card _rareCard;
		private bool _hasRare;
	}
}
