using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004B5 RID: 1205
	[UsedImplicitly]
	public sealed class FreezeBullet : Card
	{
		// Token: 0x06000FFD RID: 4093 RVA: 0x0001C5D8 File Offset: 0x0001A7D8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<FreezeBulletSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
