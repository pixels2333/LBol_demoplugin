using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003CA RID: 970
	[UsedImplicitly]
	public sealed class ChunseXiaojing : Card
	{
		// Token: 0x06000DAB RID: 3499 RVA: 0x00019996 File Offset: 0x00017B96
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield return new DrawManyCardAction(base.Value1);
			yield return new LockRandomTurnManaAction(base.Value2);
			yield break;
		}
	}
}
