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
	// Token: 0x0200047F RID: 1151
	[UsedImplicitly]
	public sealed class KoishiWake : Card
	{
		// Token: 0x06000F6A RID: 3946 RVA: 0x0001B99E File Offset: 0x00019B9E
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
