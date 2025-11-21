using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003CF RID: 975
	[UsedImplicitly]
	public sealed class DoubleLianhuadie : Card
	{
		// Token: 0x06000DBA RID: 3514 RVA: 0x00019AB0 File Offset: 0x00017CB0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<DoubleLianhuadieSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
