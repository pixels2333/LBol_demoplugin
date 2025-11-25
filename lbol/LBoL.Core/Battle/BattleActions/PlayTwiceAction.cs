using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class PlayTwiceAction : EventBattleAction<CardUsingEventArgs>
	{
		public PlayTwiceAction(Card card, CardUsingEventArgs args)
		{
			this._twiceTokenCard = card;
			base.Args = args.Clone();
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Move Clone", delegate
			{
				if (base.Battle.AddCardToFollowArea(this._twiceTokenCard) != CancelCause.None)
				{
					base.Args.IsModified = true;
				}
				base.Args.CanCancel = false;
			}, true);
			Card twiceTokenCard = this._twiceTokenCard;
			if (twiceTokenCard != null && twiceTokenCard.Zone == CardZone.FollowArea)
			{
				yield return base.CreatePhase("Play Clone", delegate
				{
					base.React(new PlayCardAction(this._twiceTokenCard, base.Args.Selector, base.Args.ConsumingMana), null, default(ActionCause?));
				}, false);
			}
			yield break;
		}
		private Card _twiceTokenCard;
	}
}
