using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200038A RID: 906
	[UsedImplicitly]
	public sealed class EvilMaid : Card
	{
		// Token: 0x06000CED RID: 3309 RVA: 0x00018C6E File Offset: 0x00016E6E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value1);
			yield return base.BuffAction<EvilMaidSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
