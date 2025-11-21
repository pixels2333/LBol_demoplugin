using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001AA RID: 426
	public sealed class TransformCardAction : EventBattleAction<CardTransformEventArgs>
	{
		// Token: 0x06000F4F RID: 3919 RVA: 0x0002938F File Offset: 0x0002758F
		public TransformCardAction(Card sourceCard, Card destinationCard)
		{
			base.Args = new CardTransformEventArgs
			{
				SourceCard = sourceCard,
				DestinationCard = destinationCard
			};
		}

		// Token: 0x06000F50 RID: 3920 RVA: 0x000293B0 File Offset: 0x000275B0
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<CardTransformEventArgs>("Transforming", base.Args, base.Battle.CardTransforming);
			yield return base.CreatePhase("Main", delegate
			{
				if (base.Args.IsCanceled)
				{
					return;
				}
				CancelCause cancelCause = base.Battle.TransformCard(base.Args.SourceCard, base.Args.DestinationCard);
				if (cancelCause != CancelCause.None)
				{
					base.Args.ForceCancelBecause(cancelCause);
					return;
				}
			}, true);
			yield return base.CreateEventPhase<CardTransformEventArgs>("Transformed", base.Args, base.Battle.CardTransformed);
			yield break;
		}
	}
}
