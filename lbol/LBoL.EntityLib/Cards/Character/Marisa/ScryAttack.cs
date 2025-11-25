using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class ScryAttack : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<ScryEventArgs>(base.Battle.Scried, new EventSequencedReactor<ScryEventArgs>(this.OnScried));
		}
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
