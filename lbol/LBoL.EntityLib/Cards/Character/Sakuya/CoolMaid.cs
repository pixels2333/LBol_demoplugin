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
	// Token: 0x02000385 RID: 901
	[UsedImplicitly]
	public sealed class CoolMaid : Card
	{
		// Token: 0x06000CD8 RID: 3288 RVA: 0x00018AE0 File Offset: 0x00016CE0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value1, false), AddCardsType.Normal);
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
