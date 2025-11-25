using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class Blizzard : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (BattleAction battleAction in base.DebuffAction<Cold>(base.Battle.AllAliveEnemies, 0, 0, 0, 0, true, 0.03f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			Card card = Enumerable.LastOrDefault<Card>(base.Battle.DrawZone);
			if (card == null)
			{
				yield break;
			}
			yield return PerformAction.ViewCard(card);
			if (!Enumerable.Contains<ManaColor>(card.Config.Colors, ManaColor.Blue))
			{
				yield break;
			}
			yield return base.BuffAction<ExtraBlizzard>(base.RawDamage, 0, 0, 0, 0.2f);
			yield break;
			yield break;
		}
	}
}
