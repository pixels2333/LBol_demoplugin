using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000442 RID: 1090
	[UsedImplicitly]
	public sealed class ScryAttack : Card
	{
		// Token: 0x06000EDE RID: 3806 RVA: 0x0001B06F File Offset: 0x0001926F
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<ScryEventArgs>(base.Battle.Scried, new EventSequencedReactor<ScryEventArgs>(this.OnScried));
		}

		// Token: 0x06000EDF RID: 3807 RVA: 0x0001B08E File Offset: 0x0001928E
		private IEnumerable<BattleAction> OnScried(ScryEventArgs args)
		{
			if (args.Cause != ActionCause.OnlyCalculate && base.Zone == CardZone.Discard)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
			}
			yield break;
		}
	}
}
