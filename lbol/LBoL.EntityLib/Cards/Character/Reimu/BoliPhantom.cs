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
	// Token: 0x020003C7 RID: 967
	[UsedImplicitly]
	public sealed class BoliPhantom : Card
	{
		// Token: 0x06000DA2 RID: 3490 RVA: 0x0001989D File Offset: 0x00017A9D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BoliPhantomSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
