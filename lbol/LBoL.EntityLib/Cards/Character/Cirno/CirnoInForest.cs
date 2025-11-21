using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004A6 RID: 1190
	[UsedImplicitly]
	public sealed class CirnoInForest : Card
	{
		// Token: 0x06000FD8 RID: 4056 RVA: 0x0001C2ED File Offset: 0x0001A4ED
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Graze>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
