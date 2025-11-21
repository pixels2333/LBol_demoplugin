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
	// Token: 0x020004AC RID: 1196
	[UsedImplicitly]
	public sealed class DeepFreeze : Card
	{
		// Token: 0x06000FE8 RID: 4072 RVA: 0x0001C451 File Offset: 0x0001A651
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<DeepFreezeSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
