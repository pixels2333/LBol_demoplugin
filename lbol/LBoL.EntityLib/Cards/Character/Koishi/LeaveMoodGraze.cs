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
	// Token: 0x02000483 RID: 1155
	[UsedImplicitly]
	public sealed class LeaveMoodGraze : Card
	{
		// Token: 0x06000F75 RID: 3957 RVA: 0x0001BA39 File Offset: 0x00019C39
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			Mood mood = (Mood)Enumerable.FirstOrDefault<StatusEffect>(base.Battle.Player.StatusEffects, (StatusEffect se) => se is Mood);
			if (mood != null)
			{
				yield return new MoodChangeAction(base.Battle.Player, mood, null);
			}
			yield break;
		}
	}
}
