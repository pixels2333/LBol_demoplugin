using System;
using System.Collections;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Modaoshu : Exhibit
	{
		protected override IEnumerator SpecialGain(PlayerUnit player)
		{
			this.OnGain(player);
			Card[] array = base.GameRun.RollCards(base.GameRun.CardRng, new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.OnlyNeutral, CardTypeWeightTable.CanBeLoot, false), base.Value1, false, false, null);
			if (array.Length != 0)
			{
				base.GameRun.UpgradeNewDeckCardOnFlags(array);
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, true, true, true)
				{
					Source = this
				};
				yield return base.GameRun.InteractionViewer.View(interaction);
				Card selectedCard = interaction.SelectedCard;
				if (selectedCard != null)
				{
					base.GameRun.AddDeckCard(selectedCard, true, new VisualSourceData
					{
						SourceType = VisualSourceType.CardSelect
					});
				}
				interaction = null;
			}
			base.Blackout = true;
			yield break;
		}
	}
}
