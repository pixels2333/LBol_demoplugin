using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001A1 RID: 417
	public sealed class ScryAction : EventBattleAction<ScryEventArgs>
	{
		// Token: 0x06000F20 RID: 3872 RVA: 0x00028CFD File Offset: 0x00026EFD
		public ScryAction(ScryInfo info)
		{
			base.Args = new ScryEventArgs
			{
				ScryInfo = info
			};
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x00028D17 File Offset: 0x00026F17
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<ScryEventArgs>("Scrying", base.Args, base.Battle.Scrying);
			int num = Math.Min(base.Args.ScryInfo.Count, base.Battle.DrawZone.Count);
			if (num > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, num, false, Enumerable.Take<Card>(base.Battle.DrawZone, num), SelectedCardHandling.Keep)
				{
					Source = base.Source
				};
				yield return base.CreatePhase("Select", delegate
				{
					this.React(new InteractionAction(interaction, false), null, default(ActionCause?));
				}, false);
				IReadOnlyList<Card> selected = interaction.SelectedCards;
				if (selected.Count > 0)
				{
					yield return base.CreatePhase("MoveToDiscard", delegate
					{
						foreach (Card card in selected)
						{
							this.React(new MoveCardAction(card, CardZone.Discard), null, default(ActionCause?));
						}
					}, false);
				}
				yield return base.CreateEventPhase<ScryEventArgs>("Scried", base.Args, base.Battle.Scried);
			}
			yield break;
		}
	}
}
