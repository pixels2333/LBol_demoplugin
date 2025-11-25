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
using LBoL.EntityLib.StatusEffects.Koishi;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class KoishiWake : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Mood mood = (Mood)Enumerable.FirstOrDefault<StatusEffect>(base.Battle.Player.StatusEffects, (StatusEffect se) => se is Mood);
			if (mood != null)
			{
				if (!(mood is MoodPassion))
				{
					if (!(mood is MoodPeace))
					{
						if (mood is MoodEpiphany)
						{
							mood.Count += base.Value1;
							mood.NotifyActivating();
							yield return new ExileCardAction(this);
						}
					}
					else
					{
						yield return base.DefenseAction(true);
					}
				}
				else
				{
					yield return new FollowAttackAction(selector, false);
				}
			}
			yield break;
		}
	}
}
