using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x02000362 RID: 866
	public sealed class LarvaCure : Card
	{
		// Token: 0x06000C7F RID: 3199 RVA: 0x00018427 File Offset: 0x00016627
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.Player.HasStatusEffect<Poison>())
			{
				yield return PerformAction.Sfx("JingHua", 0f);
				yield return PerformAction.Effect(base.Battle.Player, "JingHua", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new RemoveStatusEffectAction(base.Battle.Player.GetStatusEffect<Poison>(), true, 0.1f);
			}
			yield break;
		}
	}
}
