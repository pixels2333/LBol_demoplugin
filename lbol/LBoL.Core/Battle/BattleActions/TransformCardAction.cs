using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class TransformCardAction : EventBattleAction<CardTransformEventArgs>
	{
		public TransformCardAction(Card sourceCard, Card destinationCard)
		{
			base.Args = new CardTransformEventArgs
			{
				SourceCard = sourceCard,
				DestinationCard = destinationCard
			};
		}
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
