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
	// Token: 0x02000482 RID: 1154
	[UsedImplicitly]
	public sealed class LeaveMoodDraw : Card
	{
		// Token: 0x06000F73 RID: 3955 RVA: 0x0001BA21 File Offset: 0x00019C21
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Mood mood = (Mood)Enumerable.FirstOrDefault<StatusEffect>(base.Battle.Player.StatusEffects, (StatusEffect se) => se is Mood);
			if (mood != null)
			{
				yield return new MoodChangeAction(base.Battle.Player, mood, null);
			}
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
