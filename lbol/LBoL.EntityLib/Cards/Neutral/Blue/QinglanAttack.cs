using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x0200031E RID: 798
	[UsedImplicitly]
	public sealed class QinglanAttack : Card
	{
		// Token: 0x06000BC5 RID: 3013 RVA: 0x0001767B File Offset: 0x0001587B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new ScryAction(base.Scry);
			yield return new DrawManyCardAction(base.Value2);
			yield break;
		}
	}
}
