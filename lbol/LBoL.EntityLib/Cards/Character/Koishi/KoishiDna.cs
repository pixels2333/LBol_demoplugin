using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class KoishiDna : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<KoishiDnaSe>(base.Value1, 0, 0, 0, 0.2f);
			if (this.IsUpgraded)
			{
				KoishiDnaSe statusEffect = base.Battle.Player.GetStatusEffect<KoishiDnaSe>();
				if (statusEffect != null)
				{
					yield return statusEffect.TakeEffect();
				}
			}
			yield break;
		}
	}
}
