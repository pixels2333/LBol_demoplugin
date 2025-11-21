using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.Core.Cards
{
	// Token: 0x0200012D RID: 301
	[UsedImplicitly]
	public sealed class FollowAttackFiller : Card
	{
		// Token: 0x06000BD6 RID: 3030 RVA: 0x00021282 File Offset: 0x0001F482
		public override IEnumerable<BattleAction> AfterFollowPlayAction()
		{
			yield return new RemoveCardAction(this);
			yield break;
		}
	}
}
