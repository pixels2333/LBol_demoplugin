using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	[UsedImplicitly]
	public sealed class YijiuAttack : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageDealt, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageDealt));
		}
		private IEnumerable<BattleAction> OnPlayerDamageDealt(DamageEventArgs args)
		{
			if (args.ActionSource == this && args.Kill)
			{
				yield return new ExileCardAction(this);
				yield return base.BuffAction<Invincible>(0, base.Value1, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
