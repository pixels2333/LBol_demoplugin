using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class TimeWalk : Card
	{
		protected override string GetBaseDescription()
		{
			if (!this.Active)
			{
				return base.GetExtraDescription1;
			}
			return base.GetBaseDescription();
		}
		private bool Active
		{
			get
			{
				if (base.Battle != null)
				{
					return !Enumerable.Any<Card>(base.Battle.BattleCardUsageHistory, (Card card) => card is TimeWalk);
				}
				return true;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (this.Active)
			{
				yield return PerformAction.Effect(base.Battle.Player, "ExtraTime", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return PerformAction.Sfx("ExtraTurnLaunch", 0f);
				yield return PerformAction.Animation(base.Battle.Player, "spell", 1.6f, null, 0f, -1);
				yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
				yield return new RequestEndPlayerTurnAction();
			}
			yield break;
		}
	}
}
