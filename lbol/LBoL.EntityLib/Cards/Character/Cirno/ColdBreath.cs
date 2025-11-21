using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004A7 RID: 1191
	[UsedImplicitly]
	public sealed class ColdBreath : Card
	{
		// Token: 0x06000FDA RID: 4058 RVA: 0x0001C305 File Offset: 0x0001A505
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			bool cold = false;
			if (selector.SelectedEnemy.HasStatusEffect<Cold>())
			{
				cold = true;
			}
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd || !cold)
			{
				yield break;
			}
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
