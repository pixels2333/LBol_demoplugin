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
	// Token: 0x020003D6 RID: 982
	[UsedImplicitly]
	public sealed class HuanxiangBlock : Card
	{
		// Token: 0x06000DCA RID: 3530 RVA: 0x00019B9D File Offset: 0x00017D9D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<HuanxiangBlockSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
