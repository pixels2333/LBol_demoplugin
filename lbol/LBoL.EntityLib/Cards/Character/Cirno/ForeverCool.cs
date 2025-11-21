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
	// Token: 0x020004B4 RID: 1204
	[UsedImplicitly]
	public sealed class ForeverCool : Card
	{
		// Token: 0x06000FFB RID: 4091 RVA: 0x0001C5C0 File Offset: 0x0001A7C0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ForeverCoolSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
