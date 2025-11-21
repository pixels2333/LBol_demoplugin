using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000393 RID: 915
	[UsedImplicitly]
	public sealed class KnifeBounce : Card
	{
		// Token: 0x06000D0B RID: 3339 RVA: 0x00018E8B File Offset: 0x0001708B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
