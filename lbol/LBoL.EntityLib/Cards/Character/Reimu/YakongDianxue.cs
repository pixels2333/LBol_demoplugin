using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class YakongDianxue : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<StatusEffectEventArgs>(base.Battle.Player.StatusEffectRemoved, new EventSequencedReactor<StatusEffectEventArgs>(this.StatusEffectRemoved));
		}
		private IEnumerable<BattleAction> StatusEffectRemoved(StatusEffectEventArgs args)
		{
			if (args.Effect.Type == StatusEffectType.Positive && base.Zone == CardZone.Discard && base.Battle.HandIsNotFull)
			{
				yield return new MoveCardAction(this, CardZone.Hand);
			}
			yield break;
		}
	}
}
