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

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000481 RID: 1153
	[UsedImplicitly]
	public sealed class LeaveMoodAttack : Card
	{
		// Token: 0x06000F71 RID: 3953 RVA: 0x0001BA02 File Offset: 0x00019C02
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			Mood mood = (Mood)Enumerable.FirstOrDefault<StatusEffect>(base.Battle.Player.StatusEffects, (StatusEffect se) => se is Mood);
			if (mood != null)
			{
				yield return new MoodChangeAction(base.Battle.Player, mood, null);
			}
			yield break;
		}
	}
}
