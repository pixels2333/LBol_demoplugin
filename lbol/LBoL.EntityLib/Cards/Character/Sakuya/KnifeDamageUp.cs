using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x02000394 RID: 916
	[UsedImplicitly]
	public sealed class KnifeDamageUp : Card
	{
		// Token: 0x06000D0D RID: 3341 RVA: 0x00018EAA File Offset: 0x000170AA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<KnifeDamageUpSe>(base.Value1, 0, 0, 0, 0.2f);
			yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value2, false), AddCardsType.Normal);
			yield break;
		}
	}
}
