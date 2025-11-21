using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000335 RID: 821
	[UsedImplicitly]
	public sealed class PaluxiCurse : Card
	{
		// Token: 0x06000BFD RID: 3069 RVA: 0x00017A39 File Offset: 0x00015C39
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			EnemyUnit enemy = selector.SelectedEnemy;
			if (enemy.IsAlive)
			{
				yield return base.DebuffAction<Weak>(enemy, 0, base.Value1, 0, 0, true, 0.2f);
				yield return base.DebuffAction<Vulnerable>(enemy, 0, base.Value1, 0, 0, true, 0.2f);
			}
			Card randomCurseCard = base.GameRun.GetRandomCurseCard(base.GameRun.BattleCardRng, true);
			if (randomCurseCard != null)
			{
				yield return new AddCardsToDiscardAction(new Card[] { randomCurseCard });
			}
			yield break;
		}
	}
}
