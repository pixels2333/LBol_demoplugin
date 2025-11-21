using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002CD RID: 717
	[UsedImplicitly]
	public sealed class ModuoluoFire : Card
	{
		// Token: 0x06000AEA RID: 2794 RVA: 0x00016477 File Offset: 0x00014677
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ModuoluoFireSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
