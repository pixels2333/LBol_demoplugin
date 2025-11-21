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
	// Token: 0x020003B0 RID: 944
	[UsedImplicitly]
	public sealed class SakuyaSnow : Card
	{
		// Token: 0x06000D63 RID: 3427 RVA: 0x00019452 File Offset: 0x00017652
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value1, false), AddCardsType.Normal);
			yield break;
		}
	}
}
