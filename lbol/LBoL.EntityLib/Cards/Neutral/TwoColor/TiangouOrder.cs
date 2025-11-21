using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002B2 RID: 690
	[UsedImplicitly]
	public sealed class TiangouOrder : Card
	{
		// Token: 0x06000AA4 RID: 2724 RVA: 0x00015F4B File Offset: 0x0001414B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.DebuffAction<TiangouOrderSe>(selector.SelectedEnemy, base.Value1, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
