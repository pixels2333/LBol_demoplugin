using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000198 RID: 408
	public sealed class PlayTwiceAction : EventBattleAction<CardUsingEventArgs>
	{
		// Token: 0x06000F02 RID: 3842 RVA: 0x000287C1 File Offset: 0x000269C1
		public PlayTwiceAction(Card card, CardUsingEventArgs args)
		{
			this._twiceTokenCard = card;
			base.Args = args.Clone();
		}

		// Token: 0x06000F03 RID: 3843 RVA: 0x000287DC File Offset: 0x000269DC
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

		// Token: 0x04000694 RID: 1684
		private Card _twiceTokenCard;
	}
}
