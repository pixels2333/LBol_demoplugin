using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Koishi
{
	[UsedImplicitly]
	public sealed class KoishiPhoneSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			BattleController battle = base.Battle;
			int num = battle.FollowAttackFillerLevel + 1;
			battle.FollowAttackFillerLevel = num;
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Card.CardType == CardType.Attack)
			{
				yield return new FollowAttackAction(args.Selector, base.Level, false);
			}
			yield break;
		}
		protected override void OnRemoved(Unit unit)
		{
			BattleController battle = base.Battle;
			int num = battle.FollowAttackFillerLevel - 1;
			battle.FollowAttackFillerLevel = num;
		}
	}
}
