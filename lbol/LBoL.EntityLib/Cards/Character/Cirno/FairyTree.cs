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
	// Token: 0x020004B1 RID: 1201
	[UsedImplicitly]
	public sealed class FairyTree : Card
	{
		// Token: 0x06000FF4 RID: 4084 RVA: 0x0001C536 File Offset: 0x0001A736
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<FairyTreeSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
