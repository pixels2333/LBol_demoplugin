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
	// Token: 0x020003F9 RID: 1017
	[UsedImplicitly]
	public sealed class ReverseJiejie : Card
	{
		// Token: 0x06000E21 RID: 3617 RVA: 0x0001A291 File Offset: 0x00018491
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ReverseJiejieSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
