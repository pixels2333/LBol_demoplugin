using System;
using System.Collections;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using UnityEngine;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class SelectCard : JadeBox
	{
		protected override void OnGain(GameRunController gameRun)
		{
			base.GameRun.RemoveGamerunInitialCards();
			gameRun.AddDeckCard(Library.CreateCard<Zhukeling>(), false, null);
		}
		protected override void OnAdded()
		{
			base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationEntered, delegate(StationEventArgs args)
			{
				EntryStation entryStation = args.Station as EntryStation;
				if (entryStation != null && base.GameRun.Stages.IndexOf(entryStation.Stage) == 0)
				{
					args.Station.PreDialogs.Add(new StationDialogSource("StartupSelectCard", this));
				}
			});
		}
		[RuntimeCommand("select", "")]
		[UsedImplicitly]
		public IEnumerator Select()
		{
			Card[] array = base.GameRun.RollCards(base.GameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), 3, false, false, null);
			base.GameRun.UpgradeNewDeckCardOnFlags(array);
			MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, false, false, false)
			{
				Source = this,
				CanCancel = false
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			base.GameRun.AddDeckCards(new Card[] { interaction.SelectedCard }, false, null);
			yield return new WaitForSeconds(0.5f);
			yield break;
		}
	}
}
