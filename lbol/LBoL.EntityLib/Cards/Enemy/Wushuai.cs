using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Enemy
{
	public sealed class Wushuai : Card
	{
		public override IEnumerable<BattleAction> OnDraw()
		{
			if (!base.Battle.BattleShouldEnd)
			{
				yield return new ApplyStatusEffectAction<LockedOn>(base.Battle.Player, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, false);
				yield return new ExileCardAction(this);
			}
			yield break;
		}
	}
}
