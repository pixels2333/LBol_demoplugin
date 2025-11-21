using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000291 RID: 657
	[UsedImplicitly]
	public sealed class HuashanDefense : Card
	{
		// Token: 0x06000A4D RID: 2637 RVA: 0x00015896 File Offset: 0x00013A96
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<TempElectric>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
