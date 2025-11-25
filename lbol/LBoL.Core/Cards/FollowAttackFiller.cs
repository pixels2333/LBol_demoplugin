using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.Core.Cards
{
	[UsedImplicitly]
	public sealed class FollowAttackFiller : Card
	{
		public override IEnumerable<BattleAction> AfterFollowPlayAction()
		{
			yield return new RemoveCardAction(this);
			yield break;
		}
	}
}
