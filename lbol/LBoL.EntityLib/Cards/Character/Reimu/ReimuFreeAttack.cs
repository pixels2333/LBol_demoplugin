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
	// Token: 0x020003EF RID: 1007
	[UsedImplicitly]
	public sealed class ReimuFreeAttack : Card
	{
		// Token: 0x06000E04 RID: 3588 RVA: 0x00019FD5 File Offset: 0x000181D5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<ReimuFreeAttackSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
