using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000323 RID: 803
	[UsedImplicitly]
	public sealed class TiangouOrder : Card
	{
		// Token: 0x06000BD1 RID: 3025 RVA: 0x00017728 File Offset: 0x00015928
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.DebuffAction<TiangouOrderSe>(selector.SelectedEnemy, base.Value1, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
