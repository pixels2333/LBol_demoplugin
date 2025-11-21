using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x0200049B RID: 1179
	[UsedImplicitly]
	public sealed class BrainOverclock : Card
	{
		// Token: 0x06000FC2 RID: 4034 RVA: 0x0001C0E6 File Offset: 0x0001A2E6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			if (base.Value2 > 0)
			{
				yield return new DrawManyCardAction(base.Value2);
			}
			if (base.Value1 > 0)
			{
				yield return new LockRandomTurnManaAction(base.Value1);
			}
			yield break;
		}
	}
}
